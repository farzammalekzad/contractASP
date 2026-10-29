
using Microsoft.AspNetCore.Http;

namespace ContractInvoice.Api.Services;

public class ContractAttachmentStorageService
{
    private readonly string _storagePath;
    private readonly long _maxFileSizeBytes;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx",
            ".jpg",
            ".jpeg",
            ".png"
        };

    public ContractAttachmentStorageService(
        IConfiguration configuration)
    {
        var configuredPath = configuration[
            "FileStorage:ContractAttachmentsPath"];

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException(
                "مسیر ذخیره‌سازی پیوست‌های قرارداد تنظیم نشده است.");
        }

        _storagePath = Path.GetFullPath(configuredPath);

        var maxFileSizeMB = configuration.GetValue<int>(
            "FileStorage:MaxFileSizeMB");

        if (maxFileSizeMB <= 0)
        {
            throw new InvalidOperationException(
                "حداکثر حجم فایل باید بزرگ‌تر از صفر باشد.");
        }

        _maxFileSizeBytes = (long)maxFileSizeMB * 1024 * 1024;
    }

    public async Task<string> SaveAsync(
        int contractId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (contractId <= 0)
            throw new ArgumentException("شناسه قرارداد معتبر نیست.");

        ArgumentNullException.ThrowIfNull(file);

        if (file.Length <= 0)
            throw new InvalidOperationException("فایل خالی است.");

        if (file.Length > _maxFileSizeBytes)
            throw new InvalidOperationException(
                "حجم فایل از حد مجاز بیشتر است.");

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException(
                "پسوند فایل مجاز نیست.");

        var contractFolder = Path.Combine(
            _storagePath,
            contractId.ToString());

        Directory.CreateDirectory(contractFolder);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var fullPath = Path.Combine(
            contractFolder,
            storedFileName);

        await using var stream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous);

        await file.CopyToAsync(stream, cancellationToken);

        return storedFileName;
    }

    public string GetFullPath(
        int contractId,
        string storedFileName)
    {
        if (contractId <= 0)
            throw new ArgumentException("شناسه قرارداد معتبر نیست.");

        if (string.IsNullOrWhiteSpace(storedFileName) ||
            storedFileName != Path.GetFileName(storedFileName))
        {
            throw new ArgumentException("نام فایل معتبر نیست.");
        }

        var contractFolder = Path.GetFullPath(
            Path.Combine(_storagePath, contractId.ToString()));

        var fullPath = Path.GetFullPath(
            Path.Combine(contractFolder, storedFileName));

        if (!fullPath.StartsWith(
                contractFolder + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "مسیر فایل معتبر نیست.");
        }

        return fullPath;
    }
}
