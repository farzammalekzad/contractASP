using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.Model;

public class PaymentDocument
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string PaymentReferenceNo { get; set; } = string.Empty;

    // Stored using the application's Jalali date string convention.
    [Required]
    [MaxLength(20)]
    public string PaymentDate { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
