
using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContractInvoice.Api.Controllers;

public class ContractAttachmentsController :  Microsoft.AspNetCore.Mvc.Controller
{
    private readonly AppDbContext _context;
    private readonly ContractAttachmentStorageService _storageService;

    public ContractAttachmentsController(
        AppDbContext context,
        ContractAttachmentStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        int contractId,
        IFormFile file,
        string documentType,
        CancellationToken cancellationToken)
    {
        if (contractId <= 0)
            return BadRequest("شناسه قرارداد معتبر نیست.");

        var contractExists = await _context.Contracts
            .AnyAsync(c => c.Id == contractId, cancellationToken);

        if (!contractExists)
            return NotFound("قرارداد موردنظر پیدا نشد.");

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "لطفاً یک فایل انتخاب کنید.";
            return RedirectToAction(
                "Details", "Contracts", new { id = contractId });
        }

        var allowedDocumentTypes = new HashSet<string>
        {
            "قرارداد اصلی",
            "الحاقیه",
            "تمدید",
            "ضمانت‌نامه",
            "سایر مدارک"
        };

        if (string.IsNullOrWhiteSpace(documentType) ||
            !allowedDocumentTypes.Contains(documentType))
        {
            TempData["ErrorMessage"] = "نوع مدرک معتبر نیست.";
            return RedirectToAction(
                "Details", "Contracts", new { id = contractId });
        }

        string? storedFileName = null;

        try
        {
            storedFileName = await _storageService.SaveAsync(
                contractId, file, cancellationToken);

            var attachment = new ContractAttachment
            {
                ContractId = contractId,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                DocumentType = documentType,
                UploadedAt = DateTime.UtcNow
            };

            _context.ContractAttachments.Add(attachment);

            await _context.SaveChangesAsync(cancellationToken);

            TempData["SuccessMessage"] = "پیوست با موفقیت ثبت شد.";
        }
        catch (Exception)
        {
            if (storedFileName != null)
            {
                try
                {
                    var fullPath = _storageService.GetFullPath(
                        contractId, storedFileName);

                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                catch
                {
                    // خطای پاک‌سازی نباید خطای اصلی را بپوشاند.
                }
            }

            TempData["ErrorMessage"] =
                "ثبت پیوست انجام نشد. لطفاً دوباره تلاش کنید.";
        }

        return RedirectToAction(
            "Details", "Contracts", new { id = contractId });
    }
    [HttpGet]
    public async Task<IActionResult> Download(
        int id,
        CancellationToken cancellationToken)
    {
        var attachment = await _context.ContractAttachments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attachment == null)
            return NotFound("پیوست موردنظر پیدا نشد.");

        var fullPath = _storageService.GetFullPath(
            attachment.ContractId,
            attachment.StoredFileName);

        if (!System.IO.File.Exists(fullPath))
            return NotFound("فایل پیوست در محل ذخیره‌سازی پیدا نشد.");

        return PhysicalFile(
            fullPath,
            "application/octet-stream",
            attachment.FileName);
    }

}

