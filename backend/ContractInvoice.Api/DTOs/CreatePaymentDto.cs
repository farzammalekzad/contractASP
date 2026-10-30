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
    [Display(Name = "طلب صورت وضعیت (مبلغ دریافتی)")]
    public decimal ReceivedAmount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "حسن انجام کار")]
    public decimal GoodPerformance { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "سپرده بیمه")]
    public decimal InsuranceDeposit { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "سپرده حسن اجرای تعهدات")]
    public decimal ObligationsDeposit { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "طلب مالیات بر ارزش افزوده")]
    public decimal VatReceivable { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "حق اشعه")]
    public decimal RadiationAllowance { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "ماموریت خارج از کشور")]
    public decimal ForeignMission { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }
}