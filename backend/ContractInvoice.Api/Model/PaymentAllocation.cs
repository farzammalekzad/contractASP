namespace ContractInvoice.Api.Model;

public class PaymentAllocation
{
    public int Id { get; set; }
    public int PaymentDocumentId { get; set; }
    public int ProgressPaymentId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public AllocationType AllocationType { get; set; }
    public decimal GoodPerformance { get; set; } = 0m;
    public decimal InsuranceDeposit { get; set; } = 0m;
    public decimal ObligationsDeposit { get; set; } = 0m;
    public decimal VatReceivable { get; set; } = 0m;
    public decimal RadiationAllowance { get; set; } = 0m;
    public decimal ForeignMission { get; set; } = 0m;
    public PaymentDocument PaymentDocument { get; set; } = null!;
    public ProgressPayment ProgressPayment { get; set; } = null!;
}
