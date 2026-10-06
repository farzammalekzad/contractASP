using ContractInvoice.Api.Model;
using Microsoft.AspNetCore.Mvc;

namespace ContractInvoice.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class ContractController : ControllerBase
{
    [HttpPost]
    public IActionResult Post(Contract contract)
    {
        return Ok(contract);
    }
}