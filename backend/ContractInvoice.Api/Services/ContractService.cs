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
        // 1. Validate contract information
        ValidateContract(contract);

        // 2. Check duplicate contract number
        var exists = _context.Contracts
            .Any(c => c.ContractNo == contract.ContractNo);

        if (exists)
        {
            throw new ArgumentException(
                "شماره قرارداد قبلاً ثبت شده است.");
        }

        // 3. Set Active based on Status
        contract.Active = contract.Status == "جاری";

        // 4. Save contract
        _context.Contracts.Add(contract);

        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException(
                "خطا در ذخیره قرارداد. لطفاً اطلاعات را بررسی کنید.");
        }

        return contract;
    }

    public Contract? GetById(int id)
    {
        return _context.Contracts.Find(id);
    }

   public Contract? Update(int id, Contract updatedContract)
    {
        // Find existing contract
        var existingContract = _context.Contracts.Find(id);

        if (existingContract == null)
        {
            return null;
        }

        // Contract number cannot be changed
        if (existingContract.ContractNo != updatedContract.ContractNo)
        {
            throw new ArgumentException(
                "شماره قرارداد پس از ثبت قابل تغییر نیست.");
        }

        // Validate contract information
        ValidateContract(updatedContract);

        // Prevent changing a referenced revenue contract's type
        if (existingContract.Type == "درآمدی" &&
            updatedContract.Type != "درآمدی")
        {
            bool hasRelatedContracts = _context.Contracts.Any(c =>
                c.ContractNoRelation == existingContract.ContractNo &&
                c.Id != id);

            if (hasRelatedContracts)
            {
                throw new ArgumentException(
                    "نوع این قرارداد قابل تغییر نیست؛ زیرا قراردادهای هزینه‌ای به آن مرتبط هستند.");
            }
        }

        // Update contract information
        existingContract.Topic = updatedContract.Topic;
        existingContract.ProjectCode = updatedContract.ProjectCode;
        existingContract.Price = updatedContract.Price;

        existingContract.StartDate = updatedContract.StartDate;
        existingContract.EndDate = updatedContract.EndDate;

        existingContract.ProjectManagerPersonelCode =
            updatedContract.ProjectManagerPersonelCode;

        existingContract.ContractYear = updatedContract.ContractYear;
        existingContract.Status = updatedContract.Status;

        existingContract.SponsorPersonelCode =
            updatedContract.SponsorPersonelCode;

        existingContract.PrincipalName = updatedContract.PrincipalName;

        existingContract.EblaghLetterNo =
            updatedContract.EblaghLetterNo;

        existingContract.EblaghLetterDate =
            updatedContract.EblaghLetterDate;

        existingContract.Type = updatedContract.Type;

        existingContract.ContractNoRelation =
            updatedContract.ContractNoRelation;

        existingContract.OutSourcePrice =
            updatedContract.OutSourcePrice;

        existingContract.LogSavePersonalCode =
            updatedContract.LogSavePersonalCode;

        existingContract.AdvancePaymentAmount =
            updatedContract.AdvancePaymentAmount;

        // Automatically update Active based on Status
        existingContract.Active =
            updatedContract.Status == "جاری";

        try
        {
            _context.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException(
                "خطا در ویرایش قرارداد. لطفاً اطلاعات را بررسی کنید.");
        }

        return existingContract;
    }

    public bool DeleteById(int id)
    {
        var contract = _context.Contracts.Find(id);

        if (contract == null)
        {
            return false;
        }

        // Check if the contract has progress payments
        var hasProgressPayments = _context.ProgressPayments
            .Any(p => p.ContractId == id);

        if (hasProgressPayments)
        {
            throw new InvalidOperationException(
                "این قرارداد دارای صورت وضعیت است و قابل حذف نیست.");
        }

        // Check if other contracts reference this contract
        var hasRelatedContracts = _context.Contracts
            .Any(c =>
                c.ContractNoRelation == contract.ContractNo &&
                c.Id != id);

        if (hasRelatedContracts)
        {
            throw new InvalidOperationException(
                "این قرارداد به قراردادهای دیگری مرتبط است و قابل حذف نیست.");
        }

        // Delete contract
        _context.Contracts.Remove(contract);
        _context.SaveChanges();

        return true;
    }

    private void ValidateContract(Contract contract)
    {
        if (string.IsNullOrWhiteSpace(contract.ContractNo))
            throw new ArgumentException("شماره قرارداد الزامی است.");

        if (string.IsNullOrWhiteSpace(contract.Topic))
            throw new ArgumentException("موضوع قرارداد الزامی است.");

        string[] allowedTypes =
        {
            "درآمدی", "هزینه‌ای", "داخلی"
        };

        if (!allowedTypes.Contains(contract.Type))
            throw new ArgumentException("نوع قرارداد نامعتبر است.");

        string[] allowedStatuses =
        {
            "جاری", "خاتمه یافته", "فسخ شده"
        };

        if (!allowedStatuses.Contains(contract.Status))
            throw new ArgumentException("وضعیت قرارداد نامعتبر است.");

        if (!string.IsNullOrWhiteSpace(contract.ContractNoRelation))
        {
            if (contract.Type != "هزینه‌ای")
                throw new ArgumentException(
                    "فقط قرارداد هزینه‌ای می‌تواند قرارداد درآمدی مرتبط داشته باشد.");

            bool exists = _context.Contracts.Any(c =>
                c.ContractNo == contract.ContractNoRelation &&
                c.Type == "درآمدی");

            if (!exists)
                throw new ArgumentException(
                    "قرارداد درآمدی مرتبط یافت نشد.");
        }
    }
};
