using ContractInvoice.Api.DTOs;
using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IProgressPaymentService _progressPaymentService;

    public PaymentsController(
        IPaymentService paymentService,
        IProgressPaymentService progressPaymentService)
    {
        _paymentService = paymentService;
        _progressPaymentService = progressPaymentService;
    }

    public IActionResult Index()
    {
        var payments = _paymentService.GetAll();
        return View(payments);
    }

    public IActionResult Details(int id)
    {
        var payment = _paymentService.GetById(id);

        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);
    }

    [HttpGet]
    public IActionResult Create(int? progressPaymentId)
    {
        LoadProgressPayments(progressPaymentId);

        var dto = new CreatePaymentDto();

        if (progressPaymentId.HasValue)
        {
            dto.ProgressPaymentId = progressPaymentId.Value;
        }

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            LoadProgressPayments(dto.ProgressPaymentId);
            return View(dto);
        }

        var payment = new Payment
        {
            ProgressPaymentId = dto.ProgressPaymentId,
            PaymentDate = dto.PaymentDate,
            ReceivedAmount = dto.ReceivedAmount,
            PaymentReferenceNo = dto.PaymentReferenceNo,
            Description = dto.Description
        };

        try
        {
            _paymentService.Add(payment);

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            LoadProgressPayments(dto.ProgressPaymentId);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var payment = _paymentService.GetById(id);

        if (payment == null)
        {
            return NotFound();
        }

        var dto = new UpdatePaymentDto
        {
            ProgressPaymentId = payment.ProgressPaymentId,
            PaymentDate = payment.PaymentDate,
            ReceivedAmount = payment.ReceivedAmount,
            PaymentReferenceNo = payment.PaymentReferenceNo,
            Description = payment.Description
        };

        LoadProgressPayments(payment.ProgressPaymentId);

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, UpdatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            LoadProgressPayments(dto.ProgressPaymentId);
            return View(dto);
        }

        var updatedPayment = new Payment
        {
            ProgressPaymentId = dto.ProgressPaymentId,
            PaymentDate = dto.PaymentDate,
            ReceivedAmount = dto.ReceivedAmount,
            PaymentReferenceNo = dto.PaymentReferenceNo,
            Description = dto.Description
        };

        try
        {
            _paymentService.Update(id, updatedPayment);

            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            LoadProgressPayments(dto.ProgressPaymentId);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var payment = _paymentService.GetById(id);

        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var deleted = _paymentService.Delete(id);

        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private void LoadProgressPayments(int? selectedId = null)
    {
        var progressPayments = _progressPaymentService.GetAll();

        ViewBag.ProgressPayments = new SelectList(
            progressPayments.Select(pp => new
            {
                pp.Id,
                DisplayText =
                    $"{pp.Contract.ContractNo} - صورت وضعیت {pp.PaymentNo} - {pp.Title}"
            }),
            "Id",
            "DisplayText",
            selectedId
        );
    }
}