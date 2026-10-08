using System.ComponentModel.DataAnnotations;

public class UpdateContractDto
{
    [Required]
    public string ContractCode {get; set;}

    [Required]
    public string Title {get; set;}

    public decimal Amount {get; set;}
}