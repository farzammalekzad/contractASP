using ContractInvoice.Api.Model;
using Microsoft.AspNetCore.Mvc;
using ContractInvoice.Api.Services;
using ContractInvoice.Api.DTOs;

namespace ContractInvoice.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class ContractController : ControllerBase
{
    private readonly ContractService _contractService;
    public ContractController(ContractService contractService)
    {
        _contractService = contractService;
    }
   
    [HttpPost]
    public IActionResult Post(CreateContractDto contract)
    {
        var newContract = new Contract
        {
          ContractCode = contract.ContractCode,
          Title = contract.Title,
          Amount = contract.Amount  
        };
        try
        {
           return Ok(_contractService.Add(newContract)); 
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_contractService.GetAll());
    }
}