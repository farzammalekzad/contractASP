namespace ContractInvoice.Api.Model;
public class Contract
{
  public int Id {get; set;}
  public string ContractCode {get; set;} = string.Empty;
  public string Title {get; set;} = string.Empty;
  public decimal Amount {get; set;}  
}
