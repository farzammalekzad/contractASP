using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.DTOs;

public class CreatePaymentDto
{
    [Required]
    [Display(Name = "صورت وضعیت")]
    public int ProgressPaymentId { get; set; }

    [Required]
    [Display(Name = "تاریخ پرداخت")]
    public string PaymentDate { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "مبلغ دریافتی")]
    public decimal ReceivedAmount { get; set; }

    [Display(Name = "شماره مرجع پرداخت")]
    public string? PaymentReferenceNo { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }
}