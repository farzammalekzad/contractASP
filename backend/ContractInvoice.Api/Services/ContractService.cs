using ContractInvoice.Api.Model;

namespace ContractInvoice.Api.Services;

public class ContractService
{
    private int _nextId = 3;
    private readonly List<Contract> _contracts = new List<Contract>
    {
        new Contract
        {
            Id = 1,
            ContractCode = "C-1001",
            Title = "ساخت نیروگاه",
            Amount = 5000000000
        },
        new Contract
        {
            Id = 2,
            ContractCode = "C-1002",
            Title = "نظارت بر پروژه",
            Amount = 2500000000
        }
    };
    public List<Contract> GetAll()
    {
        return _contracts;
    }

    public Contract Add(Contract contract)
    {
        if (_contracts.Any(c => c.ContractCode == contract.ContractCode))
        {
            throw new ArgumentException("Contract Code already exists.");
        }
        contract.Id = _nextId;
        _contracts.Add(contract);
        _nextId = _nextId + 1;
        return contract;
    }

    public Contract? GetById(int id)
    {
        return _contracts.FirstOrDefault(c => c.Id == id);
    }

    public Contract? Update(int id, Contract updatedContract)
    {
        var existingContract = _contracts.FirstOrDefault(c => c.Id == id);
        if(existingContract == null)
        {
            return null;
        }
        existingContract.ContractCode = updatedContract.ContractCode;
        existingContract.Title = updatedContract.Title;
        existingContract.Amount = updatedContract.Amount;

        return existingContract;
    }

    public bool DeleteById(int id)
    {
        var contract = _contracts.FirstOrDefault(c => c.Id == id);
        
        if(contract == null)
        {
            return false;
        }
        _contracts.Remove(contract);

        return true;

    }
};