
using System.ComponentModel.DataAnnotations;

namespace ContractInvoice.Api.Model;

public class ContractAttachment
{
    public int Id { get; set; }

    // قرارداد مرتبط
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;

    // نام اصلی فایل
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    // نام ذخیره‌شده روی سرور
    [Required]
    [MaxLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    // نوع محتوا، مانند application/pdf
    [MaxLength(100)]
    public string? ContentType { get; set; }

    // حجم فایل بر حسب بایت
    public long FileSize { get; set; }

    // نوع مدرک
    [Required]
    [MaxLength(50)]
    public string DocumentType { get; set; } = "سایر مدارک";

    // تاریخ بارگذاری
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
