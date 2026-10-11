using Microsoft.AspNetCore.Mvc;
using ContractInvoice.Api.Services;
using ContractInvoice.Api.DTOs;
using ContractInvoice.Api.Migrations;
using ContractInvoice.Api.Model;
using Microsoft.AspNetCore.Components.Web;
public class ContractsController : Controller
{
    private readonly ContractService _contractService;

    public ContractsController(ContractService contractService)
    {
        _contractService = contractService;
    }

    public IActionResult Index()
    {
        var contracts = _contractService.GetAll();

        return View(contracts);
    }

    [HttpGet]
    
    public IActionResult Create()
    {
        return View();
    }

    

    [HttpPost]
    public IActionResult Create(CreateContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var contract = new Contract
        {
            ContractCode = dto.ContractCode,
            Title = dto.Title,
            Amount = dto.Amount
        };

        try
        {
            _contractService.Add(contract);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(dto.ContractCode), ex.Message);
            return View(dto);
        }

        return RedirectToAction("Index");
    }
}