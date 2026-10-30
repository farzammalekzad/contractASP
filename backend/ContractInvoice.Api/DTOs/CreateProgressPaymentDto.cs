using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.DTOs;

public class CreateProgressPaymentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "انتخاب قرارداد الزامی است.")]
    [Display(Name = "قرارداد")]
    public int ContractId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "شماره صورت وضعیت باید بزرگتر از صفر باشد.")]
    [Display(Name = "شماره صورت وضعیت")]
    public int PaymentNo { get; set; }

    [Required(ErrorMessage = "عنوان صورت وضعیت الزامی است.")]
    [MaxLength(250)]
    [Display(Name = "عنوان صورت وضعیت")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع صورت وضعیت الزامی است.")]
    [RegularExpression("^(موقت|قطعی|تعدیل)$", ErrorMessage = "نوع صورت وضعیت نامعتبر است.")]
    [MaxLength(30)]
    [Display(Name = "نوع صورت وضعیت")]
    public string PaymentType { get; set; } = string.Empty;

    [MaxLength(20)] [Display(Name = "تاریخ شروع دوره")] public string? PeriodStartDate { get; set; }
    [MaxLength(20)] [Display(Name = "تاریخ پایان دوره")] public string? PeriodEndDate { get; set; }
    [MaxLength(100)] [Display(Name = "شماره نامه درخواست")] public string? RequestLetterNo { get; set; }
    [MaxLength(20)] [Display(Name = "تاریخ نامه درخواست")] public string? RequestLetterDate { get; set; }
    [MaxLength(100)] [Display(Name = "شماره نامه تایید")] public string? ApprovalLetterNo { get; set; }
    [MaxLength(20)] [Display(Name = "تاریخ تایید")] public string? ApprovalDate { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "مبلغ پیشنهادی")]
    public decimal ProposedAmount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "مبلغ تایید شده")]
    public decimal? ApprovedAmount { get; set; }

    [MaxLength(50)] [Display(Name = "وضعیت")] public string? Status { get; set; }
}
