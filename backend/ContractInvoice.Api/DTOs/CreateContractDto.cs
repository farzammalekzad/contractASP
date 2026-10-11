using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.DTOs;

public class CreateContractDto
{
  [Required]
  [Display(Name = "کد قرارداد")]
  public string ContractCode {get; set;}

  [Display(Name = "عنوان قرارداد")]
  public string Title {get; set;}
  [Range(0,1000000)]
  [Display(Name = "مبلغ قرارداد")]
  public decimal Amount {get; set;}  
}