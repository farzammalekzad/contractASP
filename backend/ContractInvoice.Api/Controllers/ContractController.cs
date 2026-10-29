using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using ContractInvoice.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

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

    // GET: api/Contract
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_contractService.GetAll());
    }

    // GET: api/Contract/5
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

    // POST: api/Contract
    [HttpPost]
    public IActionResult Post([FromBody] CreateContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var newContract = MapToContract(dto);

            var result = _contractService.Add(newContract);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT: api/Contract/5
    [HttpPut("{id}")]
    public IActionResult Update(
        int id,
        [FromBody] UpdateContractDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedContract = MapToContract(dto);

            var result = _contractService.Update(
                id, updatedContract);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE: api/Contract/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            var deleted = _contractService.DeleteById(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DTO TO ENTITY MAPPING
    private static Contract MapToContract(
        ContractFieldsDto dto)
    {
        return new Contract
        {
            ContractNo = dto.ContractNo,
            Topic = dto.Topic,
            ProjectCode = dto.ProjectCode,
            Price = dto.Price,

            StartDate = dto.StartDate,
            EndDate = dto.EndDate,

            ProjectManagerPersonelCode =
                dto.ProjectManagerPersonelCode,

            ContractYear = dto.ContractYear,
            Status = dto.Status,

            SponsorPersonelCode =
                dto.SponsorPersonelCode,

            PrincipalName = dto.PrincipalName,

            EblaghLetterNo = dto.EblaghLetterNo,
            EblaghLetterDate = dto.EblaghLetterDate,

            Type = dto.Type,

            ContractNoRelation =
                dto.ContractNoRelation,

            OutSourcePrice = dto.OutSourcePrice,

            LogSavePersonalCode =
                dto.LogSavePersonalCode,

            AdvancePaymentAmount =
                dto.AdvancePaymentAmount,

            Active = dto.Status == "جاری"
        };
    }
}