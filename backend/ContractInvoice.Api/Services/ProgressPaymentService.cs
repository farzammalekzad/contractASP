using ContractInvoice.Api.Model;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ContractInvoice.Api.Services;

public class ProgressPaymentService : IProgressPaymentService
{
    private static readonly string[] ValidPaymentTypes = ["موقت", "قطعی", "تعدیل", "پیش پرداخت"];
    private readonly AppDbContext _context;
    private readonly DbContextOptions<AppDbContext> _options;

    public ProgressPaymentService(AppDbContext context, DbContextOptions<AppDbContext> options)
    {
        _context = context;
        _options = options;
    }

    public List<ProgressPayment> GetAll() => _context.ProgressPayments
        .Include(p => p.Contract)
        .OrderBy(p => p.Contract.ContractNo)
        .ThenBy(p => p.PaymentNo)
        .ToList();

    public ProgressPayment? GetById(int id) => _context.ProgressPayments
        .Include(p => p.Contract)
        .FirstOrDefault(p => p.Id == id);

    public List<ProgressPayment> GetByContractId(int contractId) => _context.ProgressPayments
        .Include(p => p.Contract)
        .Where(p => p.ContractId == contractId)
        .OrderBy(p => p.PaymentNo)
        .ToList();

    public ProgressPayment Add(ProgressPayment progressPayment) => Write(context =>
    {
        if (progressPayment.PaymentType == "پیش پرداخت")
            progressPayment.PaymentNo = 0;

        Validate(context, progressPayment, null);

        // Do not accept an allocation/navigation graph through the progress-payment writer.
        var created = new ProgressPayment();
        CopyFields(context, progressPayment, created);
        context.ProgressPayments.Add(created);
        context.SaveChanges();
        return created;
    });

    public ProgressPayment? Update(int id, ProgressPayment progressPayment) => Write<ProgressPayment?>(context =>
    {
        // Only invoice locks here: never acquire a document lock after a progress-payment lock.
        var existing = PaymentPersistence.LockProgressPayment(context, id);
        if (existing is null)
        {
            return null;
        }

        var hasAllocations = context.PaymentAllocations.Any(a => a.ProgressPaymentId == id);
        var allocated = context.PaymentAllocations.Where(a => a.ProgressPaymentId == id)
            .Sum(a => (decimal?)a.AllocatedAmount) ?? 0m;
        if (hasAllocations)
        {
            // Check before advance-number normalization, so crafted requests cannot bypass protection.
            if (existing.ContractId != progressPayment.ContractId || existing.PaymentNo != progressPayment.PaymentNo ||
                existing.PaymentType != progressPayment.PaymentType)
                throw new ArgumentException("Contract, progress-payment number and type cannot change while allocations exist.");
            if (!progressPayment.ApprovedAmount.HasValue || progressPayment.ApprovedAmount.Value < allocated)
                throw new ArgumentException("Approved amount cannot be cleared or reduced below the allocated total.");
        }
        if (progressPayment.PaymentType == "پیش پرداخت")
            progressPayment.PaymentNo = 0;

        Validate(context, progressPayment, id);
        context.ProgressPayments.Attach(existing);
        CopyFields(context, progressPayment, existing);
        context.SaveChanges();
        return existing;
    });

    public bool Delete(int id) => Write(context =>
    {
        var progressPayment = PaymentPersistence.LockProgressPayment(context, id);
        if (progressPayment is null)
        {
            return false;
        }

        var hasPayments = context.PaymentAllocations
        .Any(p => p.ProgressPaymentId == id);

        if (hasPayments)
        {
            throw new InvalidOperationException(
                "این صورت وضعیت دارای پرداخت ثبت شده است و قابل حذف نیست."
            );
        }

        context.ProgressPayments.Remove(progressPayment);
        context.SaveChanges();
        return true;
    }, deleting: true);

    private static void Validate(AppDbContext context, ProgressPayment progressPayment, int? currentId)
    {
        if (!context.Contracts.Any(c => c.Id == progressPayment.ContractId))
            throw new ArgumentException("Contract not found.");
        if (progressPayment.PaymentType == "پیش پرداخت")
        {
            if (progressPayment.PaymentNo != 0)
                throw new ArgumentException("Advance payment number must be zero.");
        }
        else
        {
            if (progressPayment.PaymentNo <= 0)
                throw new ArgumentException("Payment number must be greater than zero.");
        }
        if (string.IsNullOrWhiteSpace(progressPayment.Title))
            throw new ArgumentException("Title is required.");
        if (!ValidPaymentTypes.Contains(progressPayment.PaymentType))
            throw new ArgumentException("Payment type is invalid.");
        PaymentPersistence.Money(progressPayment.ProposedAmount, "ProposedAmount");
        if (progressPayment.ApprovedAmount.HasValue)
            PaymentPersistence.Money(progressPayment.ApprovedAmount.Value, "ApprovedAmount");
        if (context.ProgressPayments.Any(p => p.ContractId == progressPayment.ContractId && p.PaymentNo == progressPayment.PaymentNo && p.Id != currentId))
            throw new ArgumentException("Payment number already exists for this contract.");
    }

    private static void CopyFields(AppDbContext context, ProgressPayment source, ProgressPayment target)
    {
        target.ContractId = source.ContractId;
        target.Header_ContractNo = context.Contracts.Where(c => c.Id == source.ContractId).Select(c => c.ContractNo).Single();
        target.PaymentNo = source.PaymentNo;
        target.Title = source.Title;
        target.PaymentType = source.PaymentType;
        target.PeriodStartDate = source.PeriodStartDate;
        target.PeriodEndDate = source.PeriodEndDate;
        target.RequestLetterNo = source.RequestLetterNo;
        target.RequestLetterDate = source.RequestLetterDate;
        target.ApprovalLetterNo = source.ApprovalLetterNo;
        target.ApprovalDate = source.ApprovalDate;
        target.ProposedAmount = source.ProposedAmount;
        target.ApprovedAmount = source.ApprovedAmount;
        target.Status = source.Status;
    }

    // Keep existing controller-compatible exception contracts without leaking raw SQL errors.
    private T Write<T>(Func<AppDbContext, T> action, bool deleting = false)
    {
        try
        {
            using var context = new AppDbContext(_options);
            using var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            var result = action(context);
            transaction.Commit();
            return result;
        }
        catch (PaymentRuleException ex)
        {
            if (deleting) throw new InvalidOperationException(ex.Message);
            throw new ArgumentException(ex.Message);
        }
        catch (Exception ex) when (PaymentPersistence.Failure(ex) is not null)
        {
            var failure = PaymentPersistence.Failure(ex)!.Value;
            if (deleting) throw new InvalidOperationException(failure.Message);
            throw new ArgumentException(failure.Message);
        }
    }
}
