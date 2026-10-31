using ContractInvoice.Api.Model;

namespace ContractInvoice.Api.Services;

// Service outcomes, not MVC input models. Callers must check Succeeded before using Value.
public sealed record PaymentServiceResult<T>(bool Succeeded, T? Value, string? ErrorCode,
    string? Message, IReadOnlyList<AllocationPreview> RefreshedPreviews)
{
    public static PaymentServiceResult<T> Success(T value) => new(true, value, null, null, []);
    public static PaymentServiceResult<T> Failure(string code, string message,
        IReadOnlyList<AllocationPreview>? previews = null) => new(false, default, code, message, previews ?? []);
}

public sealed record AllocationPreview(int ProgressPaymentId, AllocationType AllocationType,
    decimal AvailableAmount, decimal AllocatedAmount, string? ReviewToken);

internal sealed class PaymentRuleException(string code, string message,
    IReadOnlyList<AllocationPreview>? previews = null) : Exception(message)
{
    public string Code { get; } = code;
    public IReadOnlyList<AllocationPreview> Previews { get; } = previews ?? [];
}
