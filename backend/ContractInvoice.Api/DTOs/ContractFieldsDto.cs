using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.DTOs;

public class ContractFieldsDto
{
    [Required(ErrorMessage = "شماره قرارداد الزامی است.")]
    [MaxLength(50)]
    [Display(Name = "شماره قرارداد")]
    public string ContractNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "موضوع قرارداد الزامی است.")]
    [Display(Name = "موضوع قرارداد")]
    public string Topic { get; set; } = string.Empty;

    public string? ProjectCode { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? Price { get; set; }

    public string? StartDate { get; set; }

    public string? EndDate { get; set; }

    public string? ProjectManagerPersonelCode { get; set; }

    public string? ContractYear { get; set; }

    [Required]
    [RegularExpression("^(جاری|خاتمه یافته|فسخ شده)$")]
    public string Status { get; set; } = "جاری";

    public string? SponsorPersonelCode { get; set; }

    public string? PrincipalName { get; set; }

    public string? EblaghLetterNo { get; set; }

    public string? EblaghLetterDate { get; set; }

    [Required]
    [RegularExpression("^(درآمدی|هزینه‌ای|داخلی)$")]
    public string Type { get; set; } = string.Empty;

    public string? ContractNoRelation { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? OutSourcePrice { get; set; }

    public string? LogSavePersonalCode { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal? AdvancePaymentAmount { get; set; }
}