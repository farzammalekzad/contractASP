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

    // INDEX
    public IActionResult Index()
    {
        return View(_contractService.GetAll());
    }

    // CREATE - GET
    [HttpGet]
    [HttpGet]
    public IActionResult Create()
    {
        LoadRelatedContracts();
        return View(new CreateContractDto());
    }
    

    // CREATE - POST
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
            var contract = MapToContract(dto);

            _contractService.Add(contract);

            TempData["SuccessMessage"] =
                "قرارداد با موفقیت ثبت شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);

            return View(dto);
        }
    }

    // DETAILS
    public IActionResult Details(int id)
    {
        var contract = _contractService.GetById(id);

        if (contract == null)
        {
            return NotFound();
        }

        return View(contract);
    }

    // EDIT - GET
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var contract = _contractService.GetById(id);

        if (contract == null)
        {
            return NotFound();
        }

        ViewBag.ContractId = id;

        var dto = new UpdateContractDto
        {
            ContractNo = contract.ContractNo,
            Topic = contract.Topic,
            ProjectCode = contract.ProjectCode,
            Price = contract.Price,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            ProjectManagerPersonelCode =
                contract.ProjectManagerPersonelCode,
            ContractYear = contract.ContractYear,
            Status = contract.Status,
            SponsorPersonelCode =
                contract.SponsorPersonelCode,
            PrincipalName = contract.PrincipalName,
            EblaghLetterNo = contract.EblaghLetterNo,
            EblaghLetterDate = contract.EblaghLetterDate,
            Type = contract.Type,
            ContractNoRelation =
                contract.ContractNoRelation,
            OutSourcePrice = contract.OutSourcePrice,
            LogSavePersonalCode =
                contract.LogSavePersonalCode,
            AdvancePaymentAmount =
                contract.AdvancePaymentAmount
        };
        LoadRelatedContracts(dto.ContractNoRelation);
        return View(dto);
    }

    // EDIT - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, UpdateContractDto dto)
    {
        ViewBag.ContractId = id;

        if (!ModelState.IsValid)
        {
            LoadRelatedContracts(dto.ContractNoRelation);
            return View(dto);
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

            TempData["SuccessMessage"] =
                "قرارداد با موفقیت ویرایش شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                string.Empty, ex.Message);
            
            LoadRelatedContracts(dto.ContractNoRelation);
            return View(dto);
        }
    }

    // DELETE - GET
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var contract = _contractService.GetById(id);

        return contract == null
            ? NotFound()
            : View(contract);
    }

    // DELETE - POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var deleted = _contractService.DeleteById(id);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "قرارداد با موفقیت حذف شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private void LoadRelatedContracts(string? selectedContractNo = null)
    {
        ViewBag.RelatedContracts = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            _contractService.GetAll()
                .Where(c => c.Type == "درآمدی" || c.Type == "داخلی")
                .OrderBy(c => c.ContractNo)
                .Select(c => new
                {
                    c.ContractNo,
                    DisplayText = c.ContractNo + " - " + c.Topic
                }),
            "ContractNo",
            "DisplayText",
            selectedContractNo
        );
    }

    // SHARED MAPPING METHOD
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