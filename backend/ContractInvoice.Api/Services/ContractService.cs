using ContractInvoice.Api.Model;
using Microsoft.EntityFrameworkCore;

namespace ContractInvoice.Api.Services;


public class ContractService
{
    
    private readonly AppDbContext _context;
    public ContractService(AppDbContext context)
    {
        _context = context;
    }
    public List<Contract> GetAll()
    {
        return _context.Contracts.ToList();
    }

    public Contract Add(Contract contract)
    {
        var exist = _context.Contracts.Any(c => c.ContractCode == contract.ContractCode);
        if (exist)
        {
            throw new ArgumentException("Contract Code already exists.");
        }

        _context.Contracts.Add(contract);
        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Unable to save the contract. Please try again.");
        }
        
        return contract;
    }

    public Contract? GetById(int id)
    {
        return _context.Contracts.Find(id);
    }

    public Contract? Update(int id, Contract updatedContract)
    {
        var existingContract = _context.Contracts.Find(id);
        if(existingContract == null)
        {
            return null;
        }
        var duplicateExists = _context.Contracts.Any(c => c.ContractCode == updatedContract.ContractCode && c.Id != id);
        if (duplicateExists)
        {
            throw new ArgumentException("Contract code already exists.");
        }
        
        existingContract.ContractCode = updatedContract.ContractCode;
        existingContract.Title = updatedContract.Title;
        existingContract.ContractType = updatedContract.ContractType;
        existingContract.ProjectCode = updatedContract.ProjectCode;
        existingContract.Employer = updatedContract.Employer;
        existingContract.ProjectManager = updatedContract.ProjectManager;
        existingContract.ProjectSponsor = updatedContract.ProjectSponsor;
        existingContract.Amount = updatedContract.Amount;
        existingContract.AdvancePaymentAmount = updatedContract.AdvancePaymentAmount;
        existingContract.NotificationLetterNo = updatedContract.NotificationLetterNo;
        existingContract.NotificationLetterDate = updatedContract.NotificationLetterDate;
        existingContract.ContractStartDate = updatedContract.ContractStartDate;
        existingContract.ContractEndDate = updatedContract.ContractEndDate;
        existingContract.RelatedRevenueContractCode = updatedContract.RelatedRevenueContractCode;

        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Unable to update the contract. Please try again.");
        }

        return existingContract;
    }

    public bool DeleteById(int id)
    {
        var contract = _context.Contracts.Find(id);
        
        if(contract == null)
        {
            return false;
        }
        
        var hasProgressPayments = _context.ProgressPayments.Any(p => p.ContractId == id);

        if (hasProgressPayments)
        {
            throw new InvalidOperationException("این قرارداد دارای صورت وضعیت است و قابل حذف نیست");
        }

        _context.Contracts.Remove(contract);
        _context.SaveChanges();

        return true;

    }
};
