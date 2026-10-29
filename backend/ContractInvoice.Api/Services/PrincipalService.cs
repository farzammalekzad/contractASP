
using ContractInvoice.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace ContractInvoice.Api.Services;

public class PrincipalService
{
    private readonly AppDbContext _context;

    public PrincipalService(AppDbContext context)
    {
        _context = context;
    }

    public List<Principal> GetAll()
    {
        return _context.Principals
            .OrderBy(p => p.Name)
            .ToList();
    }

    public Principal? GetById(int id)
    {
        return _context.Principals.Find(id);
    }

    public void Add(Principal principal)
    {
        if (string.IsNullOrWhiteSpace(principal.Name))
        {
            throw new ArgumentException("نام کارفرما الزامی است.");
        }

        bool exists = _context.Principals.Any(p =>
            p.Name == principal.Name.Trim());

        if (exists)
        {
            throw new ArgumentException("این کارفرما قبلاً ثبت شده است.");
        }

        principal.Name = principal.Name.Trim();
        _context.Principals.Add(principal);
        _context.SaveChanges();
    }

    public void Update(Principal principal)
    {
        if (string.IsNullOrWhiteSpace(principal.Name))
        {
            throw new ArgumentException("نام کارفرما الزامی است.");
        }

        var existing = _context.Principals.Find(principal.Id);

        if (existing == null)
        {
            throw new ArgumentException("کارفرما یافت نشد.");
        }

        string name = principal.Name.Trim();

        bool duplicate = _context.Principals.Any(p =>
            p.Id != principal.Id && p.Name == name);

        if (duplicate)
        {
            throw new ArgumentException("این نام کارفرما قبلاً ثبت شده است.");
        }

        existing.Name = name;
        existing.PrincipalCode = principal.PrincipalCode;
        existing.NationalId = principal.NationalId;
        existing.EconomicCode = principal.EconomicCode;
        existing.Address = principal.Address;
        existing.Phone = principal.Phone;
        existing.Active = principal.Active;

        _context.SaveChanges();
    }
}
