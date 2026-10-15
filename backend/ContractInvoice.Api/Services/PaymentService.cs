using ContractInvoice.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace ContractInvoice.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;

    public PaymentService(AppDbContext context)
    {
        _context = context;
    }

    public List<Payment> GetAll()
    {
        return _context.Payments
            .Include(p => p.ProgressPayment)
            .ThenInclude(pp => pp.Contract)
            .ToList();
    }

    public Payment? GetById(int id)
    {
        return _context.Payments
            .Include(p => p.ProgressPayment)
            .ThenInclude(pp => pp.Contract)
            .FirstOrDefault(p => p.Id == id);
    }

    public List<Payment> GetByProgressPaymentId(int progressPaymentId)
    {
        return _context.Payments
            .Where(p => p.ProgressPaymentId == progressPaymentId)
            .Include(p => p.ProgressPayment)
            .ThenInclude(pp => pp.Contract)
            .ToList();
    }

    public Payment Add(Payment payment)
    {
        var progressPaymentExists = _context.ProgressPayments
            .Any(pp => pp.Id == payment.ProgressPaymentId);

        if (!progressPaymentExists)
        {
            throw new InvalidOperationException(
                "صورت وضعیت انتخاب شده وجود ندارد."
            );
        }

        if (payment.ReceivedAmount < 0)
        {
            throw new InvalidOperationException(
                "مبلغ دریافتی نمی‌تواند منفی باشد."
            );
        }

        _context.Payments.Add(payment);
        _context.SaveChanges();

        return payment;
    }

    public Payment Update(int id, Payment updatedPayment)
    {
        var existingPayment = _context.Payments.Find(id);

        if (existingPayment == null)
        {
            throw new KeyNotFoundException("پرداخت پیدا نشد.");
        }

        var progressPaymentExists = _context.ProgressPayments
            .Any(pp => pp.Id == updatedPayment.ProgressPaymentId);

        if (!progressPaymentExists)
        {
            throw new InvalidOperationException(
                "صورت وضعیت انتخاب شده وجود ندارد."
            );
        }

        if (updatedPayment.ReceivedAmount < 0)
        {
            throw new InvalidOperationException(
                "مبلغ دریافتی نمی‌تواند منفی باشد."
            );
        }

        existingPayment.ProgressPaymentId = updatedPayment.ProgressPaymentId;
        existingPayment.PaymentDate = updatedPayment.PaymentDate;
        existingPayment.ReceivedAmount = updatedPayment.ReceivedAmount;
        existingPayment.PaymentReferenceNo = updatedPayment.PaymentReferenceNo;
        existingPayment.Description = updatedPayment.Description;

        _context.SaveChanges();

        return existingPayment;
    }

    public bool Delete(int id)
    {
        var payment = _context.Payments.Find(id);

        if (payment == null)
        {
            return false;
        }

        _context.Payments.Remove(payment);
        _context.SaveChanges();

        return true;
    }

    public decimal GetTotalPaidByProgressPaymentId(int progressPaymentId)
    {
        return _context.Payments
            .Where(p => p.ProgressPaymentId == progressPaymentId)
            .Sum(p => p.ReceivedAmount);
    }
}