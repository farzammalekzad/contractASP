using ContractInvoice.Api.DTOs;
using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ProgressPaymentsController : Controller
{
    private readonly IProgressPaymentService _progressPaymentService;
    private readonly ContractService _contractService;
    private readonly IPaymentService _paymentService;

    public ProgressPaymentsController(IProgressPaymentService progressPaymentService, ContractService contractService,
    IPaymentService paymentService)
    {
        _progressPaymentService = progressPaymentService;
        _contractService = contractService;
        _paymentService = paymentService;
    }

    public IActionResult Index() => View(_progressPaymentService.GetAll());

    public IActionResult Details(int id)
    {
        var progressPayment =
            _progressPaymentService.GetById(id);

        if (progressPayment == null)
        {
            return NotFound();
        }

        var totalPaid = _paymentService.GetTotalPaidByProgressPaymentId(id);
        var payments = _paymentService.GetByProgressPaymentId(id);

        ViewBag.Payments = payments;

        ViewBag.TotalPaid = totalPaid;

        ViewBag.RemainingAmount =
            progressPayment.ApprovedAmount.HasValue
                ? progressPayment.ApprovedAmount.Value - totalPaid
                : (decimal?)null;

        return View(progressPayment);
    }

    [HttpGet]
    public IActionResult Create(int? contractId)
    {
        PopulateContracts(contractId);
        return View(new CreateProgressPaymentDto { ContractId = contractId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateProgressPaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            PopulateContracts(dto.ContractId);
            return View(dto);
        }

        try
        {
            var contract = _contractService.GetById(dto.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError(
                    nameof(dto.ContractId),
                    "قرارداد انتخاب‌شده یافت نشد.");

                PopulateContracts(dto.ContractId);
                return View(dto);
            }

            var progressPayment = ToEntity(dto);

            progressPayment.Header_ContractNo = contract.ContractNo;

            _progressPaymentService.Add(progressPayment);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PopulateContracts(dto.ContractId);
            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var payment = _progressPaymentService.GetById(id);
        if (payment is null) return NotFound();
        PopulateContracts(payment.ContractId);
        return View(ToDto(payment));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, UpdateProgressPaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            PopulateContracts(dto.ContractId);
            return View(dto);
        }

        try
        {
            var contract = _contractService.GetById(dto.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError(
                    nameof(dto.ContractId),
                    "قرارداد انتخاب‌شده یافت نشد.");

                PopulateContracts(dto.ContractId);
                return View(dto);
            }

            var progressPayment = ToEntity(dto);
            progressPayment.Header_ContractNo = contract.ContractNo;

            if (_progressPaymentService.Update(id, progressPayment) is null)
                return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            PopulateContracts(dto.ContractId);
            return View(dto);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var payment = _progressPaymentService.GetById(id);
        return payment is null ? NotFound() : View(payment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var deleted = _progressPaymentService.Delete(id);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "صورت وضعیت با موفقیت حذف شد.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(nameof(Index));
        }
    }

    private void PopulateContracts(int? selectedContractId) => ViewBag.Contracts = new SelectList(
        _contractService.GetAll().OrderBy(c => c.ContractNo).Select(c => new { c.Id, Name = $"{c.ContractNo} - {c.Topic}" }),
        "Id", "Name", selectedContractId);

    private static ProgressPayment ToEntity(CreateProgressPaymentDto dto) => new()
    {
        ContractId = dto.ContractId, PaymentNo = dto.PaymentNo, Title = dto.Title, PaymentType = dto.PaymentType,
        PeriodStartDate = dto.PeriodStartDate, PeriodEndDate = dto.PeriodEndDate, RequestLetterNo = dto.RequestLetterNo,
        RequestLetterDate = dto.RequestLetterDate, ApprovalLetterNo = dto.ApprovalLetterNo, ApprovalDate = dto.ApprovalDate,
        ProposedAmount = dto.ProposedAmount, ApprovedAmount = dto.ApprovedAmount, Status = dto.Status
    };

    private static UpdateProgressPaymentDto ToDto(ProgressPayment p) => new()
    {
        ContractId = p.ContractId, PaymentNo = p.PaymentNo, Title = p.Title, PaymentType = p.PaymentType,
        PeriodStartDate = p.PeriodStartDate, PeriodEndDate = p.PeriodEndDate, RequestLetterNo = p.RequestLetterNo,
        RequestLetterDate = p.RequestLetterDate, ApprovalLetterNo = p.ApprovalLetterNo, ApprovalDate = p.ApprovalDate,
        ProposedAmount = p.ProposedAmount, ApprovedAmount = p.ApprovedAmount, Status = p.Status
    };
}
