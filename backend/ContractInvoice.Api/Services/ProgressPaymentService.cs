using ContractInvoice.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace ContractInvoice.Api.Services;

public class ProgressPaymentService : IProgressPaymentService
{
    private static readonly string[] ValidPaymentTypes = ["TEMPORARY", "FINAL", "ADJUSTMENT"];
    private readonly AppDbContext _context;

    public ProgressPaymentService(AppDbContext context)
    {
        _context = context;
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

    public ProgressPayment Add(ProgressPayment progressPayment)
    {
        Validate(progressPayment, null);
        _context.ProgressPayments.Add(progressPayment);
        Save("Unable to save the progress payment. Please try again.");
        return progressPayment;
    }

    public ProgressPayment? Update(int id, ProgressPayment progressPayment)
    {
        var existing = _context.ProgressPayments.Find(id);
        if (existing is null)
        {
            return null;
        }

        Validate(progressPayment, id);
        existing.ContractId = progressPayment.ContractId;
        existing.PaymentNo = progressPayment.PaymentNo;
        existing.Title = progressPayment.Title;
        existing.PaymentType = progressPayment.PaymentType;
        existing.PeriodStartDate = progressPayment.PeriodStartDate;
        existing.PeriodEndDate = progressPayment.PeriodEndDate;
        existing.RequestLetterNo = progressPayment.RequestLetterNo;
        existing.RequestLetterDate = progressPayment.RequestLetterDate;
        existing.ApprovalLetterNo = progressPayment.ApprovalLetterNo;
        existing.ApprovalDate = progressPayment.ApprovalDate;
        existing.ProposedAmount = progressPayment.ProposedAmount;
        existing.ApprovedAmount = progressPayment.ApprovedAmount;
        existing.Status = progressPayment.Status;
        Save("Unable to update the progress payment. Please try again.");
        return existing;
    }

    public bool Delete(int id)
    {
        var progressPayment = _context.ProgressPayments.Find(id);
        if (progressPayment is null)
        {
            return false;
        }

        var hasPayments = _context.Payments
        .Any(p => p.ProgressPaymentId == id);

        if (hasPayments)
        {
            throw new InvalidOperationException(
                "این صورت وضعیت دارای پرداخت ثبت شده است و قابل حذف نیست."
            );
        }

        _context.ProgressPayments.Remove(progressPayment);
        _context.SaveChanges();
        return true;
    }

    private void Validate(ProgressPayment progressPayment, int? currentId)
    {
        if (!_context.Contracts.Any(c => c.Id == progressPayment.ContractId))
            throw new ArgumentException("Contract not found.");
        if (progressPayment.PaymentNo <= 0)
            throw new ArgumentException("Payment number must be greater than zero.");
        if (string.IsNullOrWhiteSpace(progressPayment.Title))
            throw new ArgumentException("Title is required.");
        if (!ValidPaymentTypes.Contains(progressPayment.PaymentType))
            throw new ArgumentException("Payment type is invalid.");
        if (progressPayment.ProposedAmount < 0 || (progressPayment.ApprovedAmount.HasValue && progressPayment.ApprovedAmount < 0))
            throw new ArgumentException("Amounts cannot be negative.");
        if (_context.ProgressPayments.Any(p => p.ContractId == progressPayment.ContractId && p.PaymentNo == progressPayment.PaymentNo && p.Id != currentId))
            throw new ArgumentException("Payment number already exists for this contract.");
    }

    private void Save(string errorMessage)
    {
        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException(errorMessage);
        }
    }
}
