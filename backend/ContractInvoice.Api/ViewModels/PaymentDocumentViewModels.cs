using ContractInvoice.Api.DTOs;
using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;

namespace ContractInvoice.Api.ViewModels;

public sealed record PaymentDocumentDisplayModel(PaymentDocument Document, decimal TotalAllocated,
    decimal UnallocatedAmount);
public sealed record PaymentDocumentListModel(IReadOnlyList<PaymentDocumentDisplayModel> Documents, int Skip, int Take);
public sealed record EligibleProgressPaymentModel(int Id, int ContractId, string ContractNo, string Topic,
    int PaymentNo, string Title, string PaymentType, decimal ApprovedAmount, decimal TotalAllocated, decimal RemainingAmount);
public sealed record ContractOptionModel(int Id, string ContractNo, string Topic);
public sealed record PaymentDocumentFormModel(CreatePaymentDocumentDto Input,
    IReadOnlyList<ContractOptionModel> Contracts, IReadOnlyList<EligibleProgressPaymentModel> EligibleProgressPayments,
    IReadOnlyList<AllocationPreview> RefreshedPreviews, IReadOnlyList<string> BalanceWarnings);
public sealed record ProgressPaymentHistoryModel(ProgressPayment ProgressPayment,
    IReadOnlyList<PaymentAllocation> Allocations, decimal TotalAllocated, decimal? RemainingAmount);
public sealed record PaymentDocumentDeleteModel(PaymentDocumentDisplayModel Document, DeletePaymentDocumentDto Input);
