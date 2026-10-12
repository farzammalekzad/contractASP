using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.DTOs;

public class UpdateContractDto
{
    [Required]
    [MaxLength(50)]
    [Display(Name = "کد قرارداد")]
    public string ContractCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "عنوان قرارداد")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "نوع قرارداد")]
    public string ContractType { get; set; } = string.Empty;

    [Required]
    [Display(Name = "کد پروژه")]
    public string ProjectCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "کارفرما")]
    public string Employer { get; set; } = string.Empty;

    [Display(Name = "مدیر پروژه")]
    public string? ProjectManager { get; set; }

    [Display(Name = "حامی پروژه")]
    public string? ProjectSponsor { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "مبلغ قرارداد")]
    public decimal Amount { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "مبلغ پیش پرداخت")]
    public decimal? AdvancePaymentAmount { get; set; }

    [Display(Name = "شماره نامه ابلاغ")]
    public string? NotificationLetterNo { get; set; }

    [Display(Name = "تاریخ نامه ابلاغ")]
    public string? NotificationLetterDate { get; set; }

    [Display(Name = "تاریخ شروع قرارداد")]
    public string? ContractStartDate { get; set; }

    [Display(Name = "تاریخ پایان قرارداد")]
    public string? ContractEndDate { get; set; }

    [Display(Name = "کد قرارداد درآمدی مرتبط")]
    public string? RelatedRevenueContractCode { get; set; }
}