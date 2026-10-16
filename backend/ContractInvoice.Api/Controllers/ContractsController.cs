using ContractInvoice.Api.DTOs;
using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using Microsoft.AspNetCore.Mvc;

public class ContractsController : Controller
{
    private readonly ContractService _contractService;

    public ContractsController(ContractService contractService)
    {
        _contractService = contractService;
    }

    public IActionResult Index()
    {
        return View(_contractService.GetAll());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            _contractService.Add(new Contract
            {
                ContractCode = dto.ContractCode,
                Title = dto.Title,
                ContractType = dto.ContractType,
                ProjectCode = dto.ProjectCode,
                Employer = dto.Employer,
                ProjectManager = dto.ProjectManager,
                ProjectSponsor = dto.ProjectSponsor,
                Amount = dto.Amount,
                AdvancePaymentAmount = dto.AdvancePaymentAmount,
                NotificationLetterNo = dto.NotificationLetterNo,
                NotificationLetterDate = dto.NotificationLetterDate,
                ContractStartDate = dto.ContractStartDate,
                ContractEndDate = dto.ContractEndDate,
                RelatedRevenueContractCode = dto.RelatedRevenueContractCode
            });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(dto.ContractCode), ex.Message);
            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var contract = _contractService.GetById(id);
        return contract is null ? NotFound() : View(contract);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var contract = _contractService.GetById(id);
        if (contract is null)
        {
            return NotFound();
        }

        return View(new UpdateContractDto
        {
            ContractCode = contract.ContractCode,
            Title = contract.Title,
            ContractType = contract.ContractType,
            ProjectCode = contract.ProjectCode,
            Employer = contract.Employer,
            ProjectManager = contract.ProjectManager,
            ProjectSponsor = contract.ProjectSponsor,
            Amount = contract.Amount,
            AdvancePaymentAmount = contract.AdvancePaymentAmount,
            NotificationLetterNo = contract.NotificationLetterNo,
            NotificationLetterDate = contract.NotificationLetterDate,
            ContractStartDate = contract.ContractStartDate,
            ContractEndDate = contract.ContractEndDate,
            RelatedRevenueContractCode = contract.RelatedRevenueContractCode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, UpdateContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var updatedContract = _contractService.Update(id, new Contract
            {
                ContractCode = dto.ContractCode,
                Title = dto.Title,
                ContractType = dto.ContractType,
                ProjectCode = dto.ProjectCode,
                Employer = dto.Employer,
                ProjectManager = dto.ProjectManager,
                ProjectSponsor = dto.ProjectSponsor,
                Amount = dto.Amount,
                AdvancePaymentAmount = dto.AdvancePaymentAmount,
                NotificationLetterNo = dto.NotificationLetterNo,
                NotificationLetterDate = dto.NotificationLetterDate,
                ContractStartDate = dto.ContractStartDate,
                ContractEndDate = dto.ContractEndDate,
                RelatedRevenueContractCode = dto.RelatedRevenueContractCode
            });

            if (updatedContract is null)
            {
                return NotFound();
            }
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(dto.ContractCode), ex.Message);
            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var contract = _contractService.GetById(id);
        return contract is null ? NotFound() : View(contract);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            _contractService.DeleteById(id);

            TempData["SuccessMessage"] =
                "قرارداد با موفقیت حذف شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }
}
