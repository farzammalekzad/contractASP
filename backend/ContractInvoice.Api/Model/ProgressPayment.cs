namespace ContractInvoice.Api.Model;

public class ProgressPayment
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int PaymentNo { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string? PeriodStartDate { get; set; }
    public string? PeriodEndDate { get; set; }
    public string? RequestLetterNo { get; set; }
    public string? RequestLetterDate { get; set; }
    public string? ApprovalLetterNo { get; set; }
    public string? ApprovalDate { get; set; }
    public decimal ProposedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? Status { get; set; }
    public Contract Contract { get; set; } = null!;
}
