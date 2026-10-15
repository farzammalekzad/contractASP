using ContractInvoice.Api.Model;

namespace ContractInvoice.Api.Services;

public interface IPaymentService
{
    List<Payment> GetAll();
    Payment? GetById(int id);
    List<Payment> GetByProgressPaymentId(int progressPaymentId);
    Payment Add(Payment payment);
    Payment Update(int id, Payment updatedPayment);
    bool Delete(int id);
    decimal GetTotalPaidByProgressPaymentId(int progressPaymentId);
}