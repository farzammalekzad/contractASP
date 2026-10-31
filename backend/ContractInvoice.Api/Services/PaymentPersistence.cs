using ContractInvoice.Api.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ContractInvoice.Api.Services;

// Shared SQL Server locking protocol; callers own a short ReadCommitted transaction.
internal static class PaymentPersistence
{
    public static ProgressPayment? LockProgressPayment(AppDbContext context, int id)
    {
        RequireTransaction(context);
        return context.ProgressPayments.FromSqlInterpolated(
            $"SELECT * FROM [invoice] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
            .AsNoTracking().SingleOrDefault();
    }

    public static Dictionary<int, ProgressPayment> LockProgressPayments(AppDbContext context, IEnumerable<int> ids)
    {
        var result = new Dictionary<int, ProgressPayment>();
        foreach (var id in ids.Distinct().OrderBy(id => id))
        {
            var payment = LockProgressPayment(context, id);
            if (payment is null)
                throw new PaymentRuleException("NotFound", $"Progress payment {id} no longer exists.");
            result.Add(id, payment);
        }
        return result;
    }

    public static PaymentDocument LockDocument(AppDbContext context, int id, byte[]? rowVersion)
    {
        RequireTransaction(context);
        var document = context.PaymentDocuments.FromSqlInterpolated(
            $"SELECT * FROM [PaymentDocuments] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
            .AsNoTracking().SingleOrDefault()
            ?? throw new PaymentRuleException("NotFound", "Payment document no longer exists.");
        if (rowVersion is null || rowVersion.Length != 8 || !document.RowVersion.SequenceEqual(rowVersion))
            throw new PaymentRuleException("Concurrency", "The document changed. Reload it before editing or deleting.");
        return document;
    }

    public static Dictionary<int, decimal> Totals(AppDbContext context, int[] ids, int? excludedDocumentId = null)
        => context.PaymentAllocations.AsNoTracking()
            .Where(a => ids.Contains(a.ProgressPaymentId) &&
                (!excludedDocumentId.HasValue || a.PaymentDocumentId != excludedDocumentId.Value))
            .GroupBy(a => a.ProgressPaymentId)
            .Select(g => new { Id = g.Key, Amount = g.Sum(a => a.AllocatedAmount) })
            .ToDictionary(x => x.Id, x => x.Amount);

    public static void Money(decimal amount, string field, bool positive = false)
    {
        if (amount < 0 || (positive && amount == 0) || amount > 9999999999999999.99m ||
            decimal.Round(amount, 2) != amount)
            throw new PaymentRuleException("Validation", $"{field} must be {(positive ? "positive" : "nonnegative")} IRR, within decimal(18,2), without rounding.");
    }

    public static (string Code, string Message)? Failure(Exception exception)
    {
        if (exception is DbUpdateConcurrencyException)
            return ("Concurrency", "The record changed. Reload it and try again.");
        var sql = exception as SqlException ?? exception.InnerException as SqlException;
        if (sql is not null)
        {
            if (sql.Number is 2601 or 2627)
            {
                if (sql.Message.Contains("IX_PaymentDocuments_PaymentReferenceNo", StringComparison.Ordinal))
                    return ("DuplicateReference", "Payment reference number already exists.");
                if (sql.Message.Contains("IX_PaymentAllocations_PaymentDocumentId_ProgressPaymentId", StringComparison.Ordinal))
                    return ("DuplicateAllocation", "A progress payment may appear only once in a document.");
                return ("Duplicate", "A record with the same unique identifier already exists.");
            }
            if (sql.Number is 1205 or 1222 or -2)
                return ("Concurrency", "Another operation is using these records. Refresh and try again.");
            if (sql.Number == 547)
                return ("Validation", "Related records or financial constraints changed. Refresh and review the values.");
            if (sql.Number is 208 or 207)
                return ("SchemaUnavailable", "The database is not ready for the new payment module. Its migration must be applied in the approved deployment phase.");
            return ("Persistence", "The database operation could not be completed. Please try again.");
        }
        return exception is DbUpdateException
            ? ("Persistence", "The changes could not be saved. Please review the values and try again.") : null;
    }

    private static void RequireTransaction(AppDbContext context)
    {
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Payment locks require an active transaction.");
    }
}
