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
                Amount = dto.Amount
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
            Amount = contract.Amount
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
                Amount = dto.Amount
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

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        return !_contractService.DeleteById(id)
            ? NotFound()
            : RedirectToAction(nameof(Index));
    }
}
