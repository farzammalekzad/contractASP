using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.Model;

public class Payment
{
    public int Id {get; set;}

    [Required]
    public int ProgressPaymentId {get; set;}

    [Required]
    public string PaymentDate {get; set;} = string.Empty;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")] 
    public decimal ReceivedAmount {get; set;}

    public string? PaymentReferenceNo {get; set;}

    public string? Description {get; set;}

    public ProgressPayment ProgressPayment {get; set;} = null!;
}