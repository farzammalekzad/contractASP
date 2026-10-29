
using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.Model;

public class Principal
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? PrincipalCode { get; set; }

    [MaxLength(20)]
    public string? NationalId { get; set; }

    [MaxLength(30)]
    public string? EconomicCode { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    public bool Active { get; set; } = true;

    public ICollection<Contract> Contracts { get; set; }
        = new List<Contract>();
}
