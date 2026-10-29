
using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using Microsoft.AspNetCore.Mvc;

public class PrincipalsController : Controller
{
    private readonly PrincipalService _principalService;

    public PrincipalsController(PrincipalService principalService)
    {
        _principalService = principalService;
    }

    // INDEX
    public IActionResult Index()
    {
        return View(_principalService.GetAll());
    }

    // CREATE - GET
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Principal());
    }

    // CREATE - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Principal principal)
    {
        if (!ModelState.IsValid)
        {
            return View(principal);
        }

        try
        {
            _principalService.Add(principal);

            TempData["SuccessMessage"] =
                "کارفرما با موفقیت ثبت شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(principal);
        }
    }

    // EDIT - GET
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var principal = _principalService.GetById(id);

        if (principal == null)
        {
            return NotFound();
        }

        return View(principal);
    }

    // EDIT - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Principal principal)
    {
        if (id != principal.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(principal);
        }

        try
        {
            _principalService.Update(principal);

            TempData["SuccessMessage"] =
                "اطلاعات کارفرما با موفقیت ویرایش شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(principal);
        }
    }
}
