namespace ContractInvoice.Api.Model;
public class Contract
{
  public int Id {get; set;}
  public string ContractCode {get; set;}
  public string Title {get; set;}
  public decimal Amount {get; set;}  
}