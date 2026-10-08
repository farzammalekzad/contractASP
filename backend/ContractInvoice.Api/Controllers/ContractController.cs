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

    [HttpGet("{id}")]
    
    public IActionResult GetById(int id)
    {
        var contract = _contractService.GetById(id);
        if (contract == null)
        {
            return NotFound();
        }

        return Ok(contract);
    }

    [HttpPut("{id}")]

    public IActionResult Update(int id, UpdateContractDto updatedContract)
    {
        var contractToUpdate = new Contract
        {
            ContractCode = updatedContract.ContractCode,
            Title = updatedContract.Title,
            Amount = updatedContract.Amount
        };
        var contract = _contractService.Update(id, contractToUpdate);
        
        if(contract == null)
        {
            return NotFound();
        }

        return Ok(contract);

    }

    [HttpDelete]

    public IActionResult Delete(int id)
    {
        var deleted = _contractService.DeleteById(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}