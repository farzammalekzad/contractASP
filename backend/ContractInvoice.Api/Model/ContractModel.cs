using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.Model;
public class Contract
{
  public int Id {get; set;}
  public string Topic {get; set;} = string.Empty;
  public string ProjectCode {get; set;} = string.Empty;
  public string ContractNo {get; set;} = string.Empty;
  public decimal? Price {get; set;} 
  public decimal? AdvancePaymentAmount {get; set;}
  public bool Active {get; set;}
  public string? StartDate {get; set;}
  public string? EndDate {get; set;}
   public string? ProjectManagerPersonelCode {get; set;}
  public string? ContractYear {get; set;}
  [MaxLength(30)]
  public string? Status {get; set;}
  public string? SponsorPersonelCode {get; set;}
  public string PrincipalName {get; set;} = string.Empty;
  public int PrincipalId { get; set; }
  public Principal Principal { get; set; } = null!;
  public string? EblaghLetterNo {get; set;}
  public string? EblaghLetterDate {get; set;}
  [MaxLength(20)]
  public string Type {get; set;} = string.Empty;
  public string? ContractNoRelation {get; set;}
  public decimal? OutSourcePrice {get; set;}
  public string? LogSavePersonalCode {get; set;}
  public ICollection<ProgressPayment> ProgressPayments { get; set; } = new List<ProgressPayment>();
}
