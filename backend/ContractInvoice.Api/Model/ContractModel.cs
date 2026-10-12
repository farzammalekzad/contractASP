namespace ContractInvoice.Api.Model;
public class Contract
{
  public int Id {get; set;}
  public string ContractCode {get; set;} = string.Empty;
  public string Title {get; set;} = string.Empty;
  public string? ContractStartDate {get; set;}
  public string? ContractEndDate {get; set;}
  public string ContractType {get; set;} = string.Empty;
  public string ProjectCode {get; set;} = string.Empty;
  public string Employer {get; set;} = string.Empty;
  public string? ProjectManager {get; set;}
  public string? ProjectSponsor {get; set;}
  public decimal Amount {get; set;}  
  public decimal? AdvancePaymentAmount {get; set;}
  public string? NotificationLetterNo {get; set;}
  public string? NotificationLetterDate {get; set;}
  public string? RelatedRevenueContractCode {get; set;}
  public ICollection<ProgressPayment> ProgressPayments { get; set; } = new List<ProgressPayment>();
}
