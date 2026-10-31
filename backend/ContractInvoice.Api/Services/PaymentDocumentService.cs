using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using ContractInvoice.Api.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace ContractInvoice.Api.Services;

public class PaymentDocumentService : IPaymentDocumentService
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly IDataProtector _reviewProtector;
    private readonly ILogger<PaymentDocumentService> _logger;

    public PaymentDocumentService(DbContextOptions<AppDbContext> options,
        IDataProtectionProvider protectionProvider, ILogger<PaymentDocumentService> logger)
    {
        _options = options;
        _reviewProtector = protectionProvider.CreateProtector("ContractInvoice.PaymentFullReview.v1");
        _logger = logger;
    }

    public PaymentServiceResult<List<ContractInvoice.Api.ViewModels.ContractOptionModel>> GetContractOptions() => Execute(context =>
        context.Contracts.AsNoTracking().OrderBy(c => c.ContractNo)
            .Select(c => new ContractInvoice.Api.ViewModels.ContractOptionModel(c.Id, c.ContractNo, c.Topic)).ToList());

    public PaymentServiceResult<ContractInvoice.Api.ViewModels.ProgressPaymentHistoryModel> GetProgressPaymentHistory(int id) => Execute(context =>
    {
        var payment = context.ProgressPayments.AsNoTracking().Include(p => p.Contract).SingleOrDefault(p => p.Id == id)
            ?? throw new PaymentRuleException("NotFound", "Progress payment was not found.");
        var rows = context.PaymentAllocations.AsNoTracking().Include(a => a.PaymentDocument)
            .Where(a => a.ProgressPaymentId == id).OrderByDescending(a => a.PaymentDocumentId).ToList();
        var total = rows.Sum(a => a.AllocatedAmount);
        return new ContractInvoice.Api.ViewModels.ProgressPaymentHistoryModel(payment, rows, total, payment.ApprovedAmount - total);
    });
    public PaymentServiceResult<PaymentDocument> GetById(int id) => Execute(context =>
        Details(context).SingleOrDefault(d => d.Id == id)
        ?? throw new PaymentRuleException("NotFound", "Payment document was not found."));

    public PaymentServiceResult<List<PaymentDocument>> GetAll(int skip = 0, int take = 100) => Execute(context =>
    {
        if (skip < 0 || take < 1 || take > 200)
            throw new PaymentRuleException("Validation", "Use a nonnegative offset and a page size from 1 to 200.");
        return Details(context).OrderByDescending(d => d.Id).Skip(skip).Take(take).ToList();
    });

    public PaymentServiceResult<List<(ProgressPayment ProgressPayment, decimal TotalAllocated, decimal RemainingAmount)>>
        GetEligibleProgressPayments(IReadOnlyCollection<int> contractIds) => Execute(context =>
    {
        if (contractIds is null || contractIds.Any(id => id <= 0))
            throw new PaymentRuleException("Validation", "Select valid contracts.");
        var ids = contractIds.Distinct().ToArray();
        // One SQL projection; selection previews are advisory and revalidated under locks at save time.
        var rows = context.ProgressPayments.AsNoTracking().Include(p => p.Contract)
            .Where(p => ids.Contains(p.ContractId) && p.ApprovedAmount > 0)
            .Select(p => new
            {
                ProgressPayment = p,
                TotalAllocated = p.Allocations.Sum(a => (decimal?)a.AllocatedAmount) ?? 0m
            })
            .Where(x => x.ProgressPayment.ApprovedAmount > x.TotalAllocated)
            .OrderBy(x => x.ProgressPayment.ContractId).ThenBy(x => x.ProgressPayment.PaymentNo)
            .ToList();
        return rows.Select(x => (x.ProgressPayment, x.TotalAllocated,
            x.ProgressPayment.ApprovedAmount!.Value - x.TotalAllocated)).ToList();
    });

    public PaymentServiceResult<AllocationPreview> PreviewAllocation(int progressPaymentId, AllocationType allocationType,
        decimal? partialAmount = null, int? documentId = null, byte[]? rowVersion = null) => Execute(context =>
    {
        if (progressPaymentId <= 0 || documentId is <= 0 || !Enum.IsDefined(allocationType))
            throw new PaymentRuleException("Validation", "Select a valid progress payment and allocation type.");
        using var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        var document = documentId.HasValue
            ? PaymentPersistence.LockDocument(context, documentId.Value, rowVersion) : null;
        var payment = PaymentPersistence.LockProgressPayments(context, [progressPaymentId])[progressPaymentId];
        var totals = PaymentPersistence.Totals(context, [progressPaymentId], documentId);
        var available = Available(payment, totals.GetValueOrDefault(progressPaymentId), allocationType);
        var amount = allocationType == AllocationType.Full ? available : partialAmount ?? 0m;
        PaymentPersistence.Money(amount, "AllocatedAmount", positive: true);
        if (amount > available)
            throw new PaymentRuleException("Validation", "The partial amount exceeds the available balance.");
        var preview = MakePreview(progressPaymentId, allocationType, available, amount, document);
        transaction.Commit();
        return preview;
    });

    public PaymentServiceResult<(decimal TotalAllocated, decimal UnallocatedAmount)> CalculateDocumentTotals(
        decimal totalAmount, IEnumerable<decimal> allocatedAmounts) => Execute(_ =>
    {
        if (allocatedAmounts is null)
            throw new PaymentRuleException("Validation", "Allocation amounts are required.");
        return Totals(totalAmount, allocatedAmounts);
    });

    public PaymentServiceResult<PaymentDocument> Create(PaymentDocument document,
        IReadOnlyDictionary<int, string>? fullReviewTokens = null, bool fullAmountsConfirmed = false) => Execute(context =>
    {
        var proposed = CopyAndValidate(document);
        if (proposed.Id != 0 || proposed.Allocations.Any(a => a.Id != 0 || a.PaymentDocumentId != 0))
            throw new PaymentRuleException("Validation", "New documents and allocations must not specify saved IDs.");
        using var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        var ids = proposed.Allocations.Select(a => a.ProgressPaymentId).ToArray();
        var payments = PaymentPersistence.LockProgressPayments(context, ids);
        var totals = PaymentPersistence.Totals(context, ids);
        EnsureUniqueReference(context, proposed.PaymentReferenceNo, null);
        ResolveAmounts(proposed, null, [], payments, totals, fullReviewTokens, fullAmountsConfirmed);
        context.PaymentDocuments.Add(proposed);
        context.SaveChanges();
        transaction.Commit();
        return proposed;
    });

    public PaymentServiceResult<PaymentDocument> Update(int id, PaymentDocument document,
        IReadOnlyDictionary<int, string>? fullReviewTokens = null, bool fullAmountsConfirmed = false) => Execute(context =>
    {
        var proposed = CopyAndValidate(document);
        if (id <= 0 || (proposed.Id != 0 && proposed.Id != id))
            throw new PaymentRuleException("Validation", "Document ID does not match the requested edit.");
        using var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        var existing = PaymentPersistence.LockDocument(context, id, proposed.RowVersion);
        // Header lock serializes all detail changes for this document before reading old allocation IDs.
        var old = context.PaymentAllocations.Where(a => a.PaymentDocumentId == id).ToList();
        var ids = old.Select(a => a.ProgressPaymentId)
            .Concat(proposed.Allocations.Select(a => a.ProgressPaymentId)).Distinct().ToArray();
        var payments = PaymentPersistence.LockProgressPayments(context, ids);
        var totals = PaymentPersistence.Totals(context, ids, id);
        EnsureUniqueReference(context, proposed.PaymentReferenceNo, id);
        var oldById = old.ToDictionary(a => a.Id);
        foreach (var row in proposed.Allocations)
        {
            if ((row.PaymentDocumentId != 0 && row.PaymentDocumentId != id) ||
                (row.Id != 0 && !oldById.ContainsKey(row.Id)))
                throw new PaymentRuleException("Validation", "An allocation does not belong to this document.");
        }
        ResolveAmounts(proposed, existing, old, payments, totals, fullReviewTokens, fullAmountsConfirmed);

        // Preserve IDs for unchanged targets. Remove changed targets first so swaps cannot violate
        // the unique pair index temporarily. Both saves are inside the same transaction.
        var retained = proposed.Allocations.Where(a => a.Id != 0 &&
            oldById[a.Id].ProgressPaymentId == a.ProgressPaymentId).Select(a => a.Id).ToHashSet();
        context.PaymentAllocations.RemoveRange(old.Where(a => !retained.Contains(a.Id)));
        context.SaveChanges();

        context.PaymentDocuments.Attach(existing);
        existing.PaymentReferenceNo = proposed.PaymentReferenceNo;
        existing.PaymentDate = proposed.PaymentDate;
        existing.TotalAmount = proposed.TotalAmount;
        existing.Description = proposed.Description;
        // A detail-only change must also advance SQL Server's document rowversion.
        context.Entry(existing).Property(d => d.Description).IsModified = true;
        foreach (var row in proposed.Allocations)
        {
            if (retained.Contains(row.Id))
            {
                var target = oldById[row.Id];
                CopyAmounts(row, target);
                target.AllocationType = row.AllocationType;
            }
            else
            {
                row.Id = 0;
                row.PaymentDocumentId = id;
                context.PaymentAllocations.Add(row);
            }
        }
        context.SaveChanges();
        var savedRows = context.PaymentAllocations.Where(a => a.PaymentDocumentId == id).ToList();
        existing.Allocations = savedRows;
        transaction.Commit();
        return existing;
    });

    public PaymentServiceResult<bool> Delete(int id, byte[] rowVersion, bool confirmed) => Execute(context =>
    {
        if (!confirmed)
            throw new PaymentRuleException("ConfirmationRequired", "Review the document and all allocations, then confirm deletion.");
        using var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
        var document = PaymentPersistence.LockDocument(context, id, rowVersion);
        var rows = context.PaymentAllocations.Where(a => a.PaymentDocumentId == id).ToList();
        PaymentPersistence.LockProgressPayments(context, rows.Select(a => a.ProgressPaymentId));
        context.PaymentAllocations.RemoveRange(rows);
        context.PaymentDocuments.Remove(document);
        context.SaveChanges();
        transaction.Commit();
        return true;
    });

    private void ResolveAmounts(PaymentDocument proposed, PaymentDocument? existing,
        List<PaymentAllocation> old, Dictionary<int, ProgressPayment> payments, Dictionary<int, decimal> paid,
        IReadOnlyDictionary<int, string>? tokens, bool confirmed)
    {
        tokens ??= new Dictionary<int, string>();
        if (tokens.Keys.Any(id => !proposed.Allocations.Any(a => a.ProgressPaymentId == id && a.AllocationType == AllocationType.Full)))
            throw new PaymentRuleException("Validation", "A Full review token does not match a proposed Full allocation.");
        var refreshed = new List<AllocationPreview>();
        foreach (var row in proposed.Allocations)
        {
            var available = Available(payments[row.ProgressPaymentId], paid.GetValueOrDefault(row.ProgressPaymentId), row.AllocationType);
            var saved = old.SingleOrDefault(a => a.Id == row.Id && a.ProgressPaymentId == row.ProgressPaymentId);
            if (row.AllocationType == AllocationType.Full)
            {
                if (saved?.AllocationType == AllocationType.Full && !tokens.ContainsKey(row.ProgressPaymentId))
                {
                    // Submitted amounts are never authoritative for saved Full rows either.
                    row.AllocatedAmount = saved.AllocatedAmount;
                }
                else
                {
                    var review = confirmed && tokens.TryGetValue(row.ProgressPaymentId, out var token)
                        ? ReadReview(token, row.ProgressPaymentId, existing) : null;
                    if (review is null || review.Amount != available)
                        refreshed.Add(MakePreview(row.ProgressPaymentId, AllocationType.Full, available, available, existing));
                    row.AllocatedAmount = available;
                }
            }
            PaymentPersistence.Money(row.AllocatedAmount, "AllocatedAmount", positive: true);
            if (row.AllocatedAmount > available)
                throw new PaymentRuleException("Validation", $"Allocation for progress payment {row.ProgressPaymentId} exceeds its current available balance of {available:0.00} IRR. No amount was changed or saved.");
        }
        if (refreshed.Count > 0)
            throw new PaymentRuleException("ReviewRequired", "Review and confirm the refreshed Full amounts before saving. No changes were saved.", refreshed);
        Totals(proposed.TotalAmount, proposed.Allocations.Select(a => a.AllocatedAmount));
    }

    private AllocationPreview MakePreview(int progressPaymentId, AllocationType type, decimal available,
        decimal amount, PaymentDocument? document)
    {
        var token = type == AllocationType.Full
            ? _reviewProtector.Protect(JsonSerializer.Serialize(new FullReview(progressPaymentId,
                document?.Id, document is null ? null : Convert.ToBase64String(document.RowVersion),
                amount, DateTimeOffset.UtcNow.AddMinutes(30)))) : null;
        return new AllocationPreview(progressPaymentId, type, available, amount, token);
    }

    private FullReview? ReadReview(string? token, int progressPaymentId, PaymentDocument? document)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var review = JsonSerializer.Deserialize<FullReview>(_reviewProtector.Unprotect(token));
            return review is not null && review.ProgressPaymentId == progressPaymentId &&
                review.DocumentId == document?.Id &&
                review.RowVersion == (document is null ? null : Convert.ToBase64String(document.RowVersion)) &&
                review.ExpiresAt > DateTimeOffset.UtcNow ? review : null;
        }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException)
        {
            return null;
        }
    }

    private sealed record FullReview(int ProgressPaymentId, int? DocumentId, string? RowVersion,
        decimal Amount, DateTimeOffset ExpiresAt);

    private static decimal Available(ProgressPayment payment, decimal allocated, AllocationType type)
    {
        if (payment.ApprovedAmount is not > 0)
            throw new PaymentRuleException("Ineligible", $"Progress payment {payment.Id} has no positive approved amount.");
        PaymentPersistence.Money(payment.ApprovedAmount.Value, "ApprovedAmount", positive: true);
        var remaining = payment.ApprovedAmount.Value - allocated;
        if (remaining <= 0)
            throw new PaymentRuleException("Ineligible", $"Progress payment {payment.Id} has no remaining payable balance.",
                [new AllocationPreview(payment.Id, type, remaining, 0m, null)]);
        return remaining;
    }

    private static (decimal TotalAllocated, decimal UnallocatedAmount) Totals(decimal total, IEnumerable<decimal> amounts)
    {
        PaymentPersistence.Money(total, "TotalAmount", positive: true);
        var allocated = 0m;
        foreach (var amount in amounts)
        {
            PaymentPersistence.Money(amount, "AllocatedAmount", positive: true);
            if (amount > total - allocated)
                throw new PaymentRuleException("Validation", "Allocations exceed the document total.");
            allocated += amount;
        }
        return (allocated, total - allocated);
    }

    private static void EnsureUniqueReference(AppDbContext context, string reference, int? currentId)
    {
        // SQL equality uses the same collation as the unique index. The index handles races.
        if (context.PaymentDocuments.Any(d => d.PaymentReferenceNo == reference &&
            (!currentId.HasValue || d.Id != currentId.Value)))
            throw new PaymentRuleException("DuplicateReference", "Payment reference number already exists.");
    }

    private static PaymentDocument CopyAndValidate(PaymentDocument? source)
    {
        if (source is null) throw new PaymentRuleException("Validation", "A document is required.");
        var reference = source.PaymentReferenceNo?.Trim() ?? "";
        if (reference.Length is < 1 or > 100)
            throw new PaymentRuleException("Validation", "Payment reference is required and must not exceed 100 characters.");
        var date = ValidateDate(source.PaymentDate);
        if (source.Description?.Length > 500)
            throw new PaymentRuleException("Validation", "Description must not exceed 500 characters.");
        PaymentPersistence.Money(source.TotalAmount, "TotalAmount", positive: true);
        if (source.Allocations is null || source.Allocations.Count == 0)
            throw new PaymentRuleException("Validation", "At least one allocation is required.");
        var document = new PaymentDocument
        {
            Id = source.Id, PaymentReferenceNo = reference, PaymentDate = date, TotalAmount = source.TotalAmount,
            Description = string.IsNullOrWhiteSpace(source.Description) ? null : source.Description.Trim(),
            RowVersion = source.RowVersion?.ToArray() ?? []
        };
        foreach (var row in source.Allocations)
        {
            if (row is null || row.Id < 0 || row.ProgressPaymentId <= 0 || !Enum.IsDefined(row.AllocationType))
                throw new PaymentRuleException("Validation", "Every allocation must have a valid progress payment and mode.");
            foreach (var value in new[] { row.GoodPerformance, row.InsuranceDeposit, row.ObligationsDeposit,
                row.VatReceivable, row.RadiationAllowance, row.ForeignMission })
                PaymentPersistence.Money(value, "Allocation financial fields");
            // Full submitted amounts are ignored; the reviewed/stored server amount is used instead.
            if (row.AllocationType == AllocationType.Partial)
                PaymentPersistence.Money(row.AllocatedAmount, "AllocatedAmount", positive: true);
            var copy = new PaymentAllocation { Id = row.Id, PaymentDocumentId = row.PaymentDocumentId,
                ProgressPaymentId = row.ProgressPaymentId, AllocationType = row.AllocationType };
            CopyAmounts(row, copy);
            document.Allocations.Add(copy);
        }
        if (document.Allocations.GroupBy(a => a.ProgressPaymentId).Any(g => g.Count() > 1) ||
            document.Allocations.Where(a => a.Id != 0).GroupBy(a => a.Id).Any(g => g.Count() > 1))
            throw new PaymentRuleException("DuplicateAllocation", "Each progress payment and saved allocation may appear only once.");
        return document;
    }

    private static string ValidateDate(string? value)
    {
        var date = (value ?? "").Trim();
        date = string.Concat(date.Select(c => c is >= '\u06f0' and <= '\u06f9' ? (char)('0' + c - '\u06f0') :
            c is >= '\u0660' and <= '\u0669' ? (char)('0' + c - '\u0660') : c));
        if (date.Length > 20 || !Regex.IsMatch(date, @"^[0-9]{4}/[0-9]{1,2}/[0-9]{1,2}$"))
            throw new PaymentRuleException("Validation", "Payment date must be a valid Jalali year/month/day string (maximum 20 characters).");
        var parts = date.Split('/').Select(p => int.Parse(p, CultureInfo.InvariantCulture)).ToArray();
        try { _ = new PersianCalendar().ToDateTime(parts[0], parts[1], parts[2], 0, 0, 0, 0); }
        catch (ArgumentOutOfRangeException)
        { throw new PaymentRuleException("Validation", "Payment date is not a valid Jalali date."); }
        return date; // Validation only: storage remains Jalali text, not Gregorian DateTime.
    }

    private static void CopyAmounts(PaymentAllocation source, PaymentAllocation target)
    {
        target.AllocatedAmount = source.AllocatedAmount;
        target.GoodPerformance = source.GoodPerformance;
        target.InsuranceDeposit = source.InsuranceDeposit;
        target.ObligationsDeposit = source.ObligationsDeposit;
        target.VatReceivable = source.VatReceivable;
        target.RadiationAllowance = source.RadiationAllowance;
        target.ForeignMission = source.ForeignMission;
    }

    private static IQueryable<PaymentDocument> Details(AppDbContext context) => context.PaymentDocuments
        .AsNoTrackingWithIdentityResolution().Include(d => d.Allocations)
        .ThenInclude(a => a.ProgressPayment).ThenInclude(p => p.Contract);

    private PaymentServiceResult<T> Execute<T>(Func<AppDbContext, T> action)
    {
        try
        {
            // No failed writes or previously tracked entities leak into subsequent calls.
            using var context = new AppDbContext(_options);
            return PaymentServiceResult<T>.Success(action(context));
        }
        catch (PaymentRuleException ex)
        {
            return PaymentServiceResult<T>.Failure(ex.Code, ex.Message, ex.Previews);
        }
        catch (Exception ex) when (PaymentPersistence.Failure(ex) is not null)
        {
            _logger.LogWarning(ex, "Payment document operation failed.");
            var failure = PaymentPersistence.Failure(ex)!.Value;
            return PaymentServiceResult<T>.Failure(failure.Code, failure.Message);
        }
    }
}

