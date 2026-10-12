using ContractInvoice.Api.Model;

namespace ContractInvoice.Api.Services;

public interface IProgressPaymentService
{
    List<ProgressPayment> GetAll();
    ProgressPayment? GetById(int id);
    List<ProgressPayment> GetByContractId(int contractId);
    ProgressPayment Add(ProgressPayment progressPayment);
    ProgressPayment? Update(int id, ProgressPayment progressPayment);
    bool Delete(int id);
}
