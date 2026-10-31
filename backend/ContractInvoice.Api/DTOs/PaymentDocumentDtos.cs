using System.ComponentModel.DataAnnotations;
using ContractInvoice.Api.Model;

namespace ContractInvoice.Api.DTOs;

public class CreatePaymentDocumentDto
{
    [Required, StringLength(100)] public string PaymentReferenceNo { get; set; } = "";
    [Required, StringLength(20)] public string PaymentDate { get; set; } = "";
    public decimal TotalAmount { get; set; }
    [StringLength(500)] public string? Description { get; set; }
    [Required, MinLength(1)] public List<PaymentAllocationDto> Allocations { get; set; } = [];
    public List<int> SelectedContractIds { get; set; } = [];
    public bool FullAmountsConfirmed { get; set; }
}

public class UpdatePaymentDocumentDto : CreatePaymentDocumentDto
{
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
}

public class PaymentAllocationDto
{
    [Range(0, int.MaxValue)] public int Id { get; set; }
    [Range(1, int.MaxValue)] public int ProgressPaymentId { get; set; }
    [EnumDataType(typeof(AllocationType))] public AllocationType AllocationType { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal GoodPerformance { get; set; }
    public decimal InsuranceDeposit { get; set; }
    public decimal ObligationsDeposit { get; set; }
    public decimal VatReceivable { get; set; }
    public decimal RadiationAllowance { get; set; }
    public decimal ForeignMission { get; set; }
    public bool ReselectFull { get; set; }
    public string? ReviewToken { get; set; }
}

public class DeletePaymentDocumentDto
{
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
    public bool Confirmed { get; set; }
}

public class PreviewPaymentAllocationDto
{
    [Range(1, int.MaxValue)] public int ProgressPaymentId { get; set; }
    [EnumDataType(typeof(AllocationType))] public AllocationType AllocationType { get; set; }
    public decimal? PartialAmount { get; set; }
    [Range(1, int.MaxValue)] public int? DocumentId { get; set; }
    public byte[]? RowVersion { get; set; }
}
