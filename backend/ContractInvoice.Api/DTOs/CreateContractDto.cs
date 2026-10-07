using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.DTOs;

public class CreateContractDto
{
  [Required]
  public string ContractCode {get; set;}
  public string Title {get; set;}
  [Range(0,1000000)]
  public decimal Amount {get; set;}  
}