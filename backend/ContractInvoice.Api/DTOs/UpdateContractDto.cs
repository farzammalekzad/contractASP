using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.DTOs;

public class UpdateContractDto
{
    [Required]
    [Display(Name = "کد قرارداد")]
    public string ContractCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "عنوان قرارداد")]
    public string Title { get; set; } = string.Empty;

    [Range(0, 1000000)]
    [Display(Name = "مبلغ قرارداد")]
    public decimal Amount { get; set; }
}
