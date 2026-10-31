using ContractInvoice.Api.DTOs;
using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using ContractInvoice.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class PaymentsController(IPaymentDocumentService service) : Controller
{
    [HttpGet]
    public IActionResult Index(int skip = 0, int take = 100)
    {
        var result = service.GetAll(skip, take);
        if (!result.Succeeded) return Failure(result);
        return View(new PaymentDocumentListModel(result.Value!.Select(Display).ToList(), skip, take));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var result = service.GetById(id);
        return result.Succeeded ? View(Display(result.Value!)) : Failure(result);
    }

    [HttpGet]
    public IActionResult Create(int? progressPaymentId)
    {
        var input = new CreatePaymentDocumentDto();
        if (progressPaymentId.HasValue)
        {
            var history = service.GetProgressPaymentHistory(progressPaymentId.Value);
            if (!history.Succeeded) ModelState.AddModelError("", history.Message!);
            else
            {
                input.SelectedContractIds.Add(history.Value!.ProgressPayment.ContractId);
                var eligible = service.GetEligibleProgressPayments(input.SelectedContractIds);
                if (!eligible.Succeeded) ModelState.AddModelError("", eligible.Message!);
                else if (eligible.Value!.Any(p => p.ProgressPayment.Id == progressPaymentId))
                    input.Allocations.Add(new PaymentAllocationDto { ProgressPaymentId = progressPaymentId.Value, AllocationType = AllocationType.Full, ReselectFull = true });
                else ModelState.AddModelError("", "صورت وضعیت انتخاب‌شده مانده قابل پرداخت ندارد. صورت وضعیت دیگری انتخاب کنید.");
            }
        }
        return Form("Create", input);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([Bind(Prefix = "Input")] CreatePaymentDocumentDto dto)
    {
        ValidateRows(dto);
        if (!ModelState.IsValid) return Form("Create", dto);
        var result = service.Create(Entity(dto), Tokens(dto), dto.FullAmountsConfirmed);
        return result.Succeeded ? RedirectToAction(nameof(Details), new { id = result.Value!.Id }) : SaveFailure("Create", dto, result);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var result = service.GetById(id);
        if (!result.Succeeded) return Failure(result);
        var document = result.Value!;
        var input = new UpdatePaymentDocumentDto
        {
            PaymentReferenceNo = document.PaymentReferenceNo, PaymentDate = document.PaymentDate,
            TotalAmount = document.TotalAmount, Description = document.Description, RowVersion = document.RowVersion,
            SelectedContractIds = document.Allocations.Select(a => a.ProgressPayment.ContractId).Distinct().ToList(),
            Allocations = document.Allocations.Select(a => new PaymentAllocationDto
            {
                Id = a.Id, ProgressPaymentId = a.ProgressPaymentId, AllocationType = a.AllocationType,
                AllocatedAmount = a.AllocatedAmount, GoodPerformance = a.GoodPerformance,
                InsuranceDeposit = a.InsuranceDeposit, ObligationsDeposit = a.ObligationsDeposit,
                VatReceivable = a.VatReceivable, RadiationAllowance = a.RadiationAllowance, ForeignMission = a.ForeignMission
            }).ToList()
        };
        var warnings = new List<string>();
        foreach (var row in document.Allocations)
        {
            // Advisory validation preserves saved Full amounts; it never recalculates them.
            var check = service.PreviewAllocation(row.ProgressPaymentId, AllocationType.Partial, row.AllocatedAmount, id, document.RowVersion);
            if (!check.Succeeded) warnings.Add($"{row.ProgressPaymentId}: {check.Message}");
        }
        return Form("Edit", input, warnings: warnings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind(Prefix = "Input")] UpdatePaymentDocumentDto dto)
    {
        ValidateRows(dto);
        if (!ModelState.IsValid) return Form("Edit", dto);
        var result = service.Update(id, Entity(dto), Tokens(dto), dto.FullAmountsConfirmed);
        return result.Succeeded ? RedirectToAction(nameof(Details), new { id }) : SaveFailure("Edit", dto, result);
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        var result = service.GetById(id);
        return result.Succeeded
            ? View(new PaymentDocumentDeleteModel(Display(result.Value!), new DeletePaymentDocumentDto { RowVersion = result.Value!.RowVersion }))
            : Failure(result);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id, [Bind(Prefix = "Input")] DeletePaymentDocumentDto dto)
    {
        if (ModelState.IsValid)
        {
            var result = service.Delete(id, dto.RowVersion, dto.Confirmed);
            if (result.Succeeded) return RedirectToAction(nameof(Index));
            if (result.ErrorCode == "NotFound") return NotFound();
            ModelState.AddModelError("", result.Message!);
        }
        // Keep the submitted version: a stale delete must never be silently retried with a new version.
        var current = service.GetById(id);
        return current.Succeeded ? View("Delete", new PaymentDocumentDeleteModel(Display(current.Value!), dto)) : Failure(current);
    }

    [HttpGet]
    public IActionResult EligibleProgressPayments([FromQuery] List<int> contractIds)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = service.GetEligibleProgressPayments(contractIds);
        return result.Succeeded ? Json(result.Value!.Select(Eligible).Select(WireOption)) : Failure(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult PreviewAllocation(PreviewPaymentAllocationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = service.PreviewAllocation(dto.ProgressPaymentId, dto.AllocationType, dto.PartialAmount, dto.DocumentId, dto.RowVersion);
        return result.Succeeded ? Json(new { result.Value!.ProgressPaymentId, result.Value.AllocationType, availableAmount = Money(result.Value.AvailableAmount), allocatedAmount = Money(result.Value.AllocatedAmount), result.Value.ReviewToken }) : Failure(result);
    }

    private static string Money(decimal value) => value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
    private static object WireOption(EligibleProgressPaymentModel p) => new { p.Id, p.ContractId, p.ContractNo, p.Topic, p.PaymentNo, p.Title, p.PaymentType,
        approvedAmount = Money(p.ApprovedAmount), totalAllocated = Money(p.TotalAllocated), remainingAmount = Money(p.RemainingAmount) };

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Totals(decimal totalAmount, List<decimal> allocatedAmounts)
    {
        if (!ModelState.IsValid) return BadRequest(new { message = "مبالغ را با حداکثر دو رقم اعشار وارد کنید." });
        var result = service.CalculateDocumentTotals(totalAmount, allocatedAmounts);
        return result.Succeeded ? Json(new { totalAmount = Money(totalAmount), totalAllocated = Money(result.Value.TotalAllocated), unallocatedAmount = Money(result.Value.UnallocatedAmount) }) : Failure(result);
    }
    private static string FriendlyError(string? code, string? detail) => code switch
    {
        "Concurrency" => "سند تغییر کرده یا در حال استفاده است. اطلاعات فرم حفظ شده است؛ نسخه جدید را بارگذاری و دوباره بررسی کنید.",
        "ReviewRequired" => "مبالغ کامل نیاز به بررسی و تأیید دوباره دارند. پیش‌نمایش تازه را بررسی کنید.",
        "NotFound" => "سند یا یکی از صورت وضعیت‌ها دیگر موجود نیست. انتخاب‌ها را بررسی کنید.",
        "DuplicateReference" => "شماره مرجع تکراری است. شماره دیگری وارد کنید.",
        "DuplicateAllocation" => "هر صورت وضعیت فقط یک بار در سند قابل تخصیص است.",
        _ => "ذخیره انجام نشد. مقادیر را بررسی کنید. " + detail
    };
    private void ValidateRows(CreatePaymentDocumentDto dto)
    {
        if (dto.Allocations is null || dto.Allocations.Any(a => a is null))
        {
            ModelState.AddModelError("Input.Allocations", "ردیف تخصیص نامعتبر است.");
            dto.Allocations = dto.Allocations?.Where(a => a is not null).ToList() ?? [];
        }
    }
    private IActionResult SaveFailure(string view, CreatePaymentDocumentDto dto, PaymentServiceResult<PaymentDocument> result)
    {
        ModelState.AddModelError("", FriendlyError(result.ErrorCode, result.Message));
        // Never reuse confirmation after a failed save. Retain all other posted values and the stale version.
        dto.FullAmountsConfirmed = false;
        ModelState.Remove("Input.FullAmountsConfirmed");
        foreach (var preview in result.RefreshedPreviews.Where(p => p.ReviewToken != null))
        {
            for (var i = 0; i < dto.Allocations.Count; i++)
            {
                var row = dto.Allocations[i];
                if (row.ProgressPaymentId != preview.ProgressPaymentId) continue;
                row.ReviewToken = preview.ReviewToken;
                row.ReselectFull = true;
                row.AllocatedAmount = preview.AllocatedAmount;
                foreach (var field in new[] { "ReviewToken", "ReselectFull", "AllocatedAmount" })
                    ModelState.Remove($"Input.Allocations[{i}].{field}");
            }
        }
        return Form(view, dto, result.RefreshedPreviews);
    }

    private IActionResult Form(string view, CreatePaymentDocumentDto input,
        IReadOnlyList<AllocationPreview>? previews = null, IReadOnlyList<string>? warnings = null)
    {
        input.SelectedContractIds ??= [];
        var contracts = service.GetContractOptions();
        var eligible = service.GetEligibleProgressPayments(input.SelectedContractIds ?? []);
        if (!contracts.Succeeded) ModelState.AddModelError("", contracts.Message!);
        if (!eligible.Succeeded) ModelState.AddModelError("", eligible.Message!);
        var rowOptions = new List<EligibleProgressPaymentModel>();
        int? documentId = int.TryParse(RouteData.Values["id"]?.ToString(), out var parsedId) ? parsedId : null;
        foreach (var row in input.Allocations)
        {
            var history = service.GetProgressPaymentHistory(row.ProgressPaymentId);
            if (!history.Succeeded) continue;
            var h = history.Value!;
            var preview = service.PreviewAllocation(row.ProgressPaymentId, AllocationType.Full, documentId: documentId,
                rowVersion: (input as UpdatePaymentDocumentDto)?.RowVersion);
            var p = h.ProgressPayment;
            rowOptions.Add(new(p.Id, p.ContractId, p.Contract.ContractNo, p.Contract.Topic, p.PaymentNo, p.Title, p.PaymentType,
                p.ApprovedAmount ?? 0m, h.TotalAllocated, preview.Succeeded ? preview.Value!.AvailableAmount : h.RemainingAmount ?? 0m));
        }
        ViewData["RowOptions"] = rowOptions;
        return View(view, new PaymentDocumentFormModel(input, contracts.Value ?? [],
            eligible.Value?.Select(Eligible).ToList() ?? [], previews ?? [], warnings ?? []));
    }

    private PaymentDocumentDisplayModel Display(PaymentDocument document)
    {
        var totals = service.CalculateDocumentTotals(document.TotalAmount, document.Allocations.Select(a => a.AllocatedAmount));
        if (!totals.Succeeded) throw new InvalidOperationException(totals.Message);
        return new(document, totals.Value.TotalAllocated, totals.Value.UnallocatedAmount);
    }

    private static EligibleProgressPaymentModel Eligible((ProgressPayment ProgressPayment, decimal TotalAllocated, decimal RemainingAmount) row)
    {
        var p = row.ProgressPayment;
        return new(p.Id, p.ContractId, p.Contract.ContractNo, p.Contract.Topic, p.PaymentNo, p.Title, p.PaymentType,
            p.ApprovedAmount!.Value, row.TotalAllocated, row.RemainingAmount);
    }

    private static Dictionary<int, string> Tokens(CreatePaymentDocumentDto dto) => dto.Allocations
        .Where(a => a.AllocationType == AllocationType.Full && (a.ReselectFull || !string.IsNullOrEmpty(a.ReviewToken)))
        .GroupBy(a => a.ProgressPaymentId).ToDictionary(g => g.Key, g => g.First().ReviewToken ?? "");

    private static PaymentDocument Entity(CreatePaymentDocumentDto dto) => new()
    {
        PaymentReferenceNo = dto.PaymentReferenceNo, PaymentDate = dto.PaymentDate, TotalAmount = dto.TotalAmount,
        Description = dto.Description, RowVersion = (dto as UpdatePaymentDocumentDto)?.RowVersion ?? [],
        Allocations = dto.Allocations.Select(a => new PaymentAllocation
        {
            Id = a.Id, ProgressPaymentId = a.ProgressPaymentId, AllocationType = a.AllocationType, AllocatedAmount = a.AllocatedAmount,
            GoodPerformance = a.GoodPerformance, InsuranceDeposit = a.InsuranceDeposit, ObligationsDeposit = a.ObligationsDeposit,
            VatReceivable = a.VatReceivable, RadiationAllowance = a.RadiationAllowance, ForeignMission = a.ForeignMission
        }).ToList()
    };

    private IActionResult Failure<T>(PaymentServiceResult<T> result) => StatusCode(result.ErrorCode switch
    {
        "NotFound" => 404, "Concurrency" or "ReviewRequired" => 409,
        "Persistence" or "SchemaUnavailable" => 503, _ => 400
    }, new { result.ErrorCode, result.Message, result.RefreshedPreviews });
}
