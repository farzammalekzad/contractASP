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
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal GoodPerformance { get; set; } = 0;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal InsuranceDeposit { get; set; } = 0;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal ObligationsDeposit { get; set; } = 0;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal VatReceivable { get; set; } = 0;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal RadiationAllowance { get; set; } = 0;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal ForeignMission { get; set; } = 0;

    public string? Description {get; set;}

    public ProgressPayment ProgressPayment {get; set;} = null!;
}