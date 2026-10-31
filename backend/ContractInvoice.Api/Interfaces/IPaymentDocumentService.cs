using ContractInvoice.Api.Model;

namespace ContractInvoice.Api.Services;

public interface IPaymentDocumentService
{
    PaymentServiceResult<ContractInvoice.Api.ViewModels.ProgressPaymentHistoryModel> GetProgressPaymentHistory(int id);
    PaymentServiceResult<List<ContractInvoice.Api.ViewModels.ContractOptionModel>> GetContractOptions();
    // A token keyed by ProgressPaymentId explicitly requests Full calculation/reselection.
    // Unchanged saved Full rows must omit a token. Tokens require explicit user confirmation.
    PaymentServiceResult<PaymentDocument> Create(PaymentDocument document,
        IReadOnlyDictionary<int, string>? fullReviewTokens = null, bool fullAmountsConfirmed = false);
    PaymentServiceResult<PaymentDocument> Update(int id, PaymentDocument document,
        IReadOnlyDictionary<int, string>? fullReviewTokens = null, bool fullAmountsConfirmed = false);
    PaymentServiceResult<PaymentDocument> GetById(int id);
    PaymentServiceResult<List<PaymentDocument>> GetAll(int skip = 0, int take = 100);
    PaymentServiceResult<bool> Delete(int id, byte[] rowVersion, bool confirmed);
    PaymentServiceResult<List<(ProgressPayment ProgressPayment, decimal TotalAllocated, decimal RemainingAmount)>>
        GetEligibleProgressPayments(IReadOnlyCollection<int> contractIds);
    PaymentServiceResult<AllocationPreview> PreviewAllocation(int progressPaymentId, AllocationType allocationType,
        decimal? partialAmount = null, int? documentId = null, byte[]? rowVersion = null);
    PaymentServiceResult<(decimal TotalAllocated, decimal UnallocatedAmount)> CalculateDocumentTotals(
        decimal totalAmount, IEnumerable<decimal> allocatedAmounts);
}

