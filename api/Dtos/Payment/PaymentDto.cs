using api.Domain.Enums;

namespace api.Dtos.Payment;

public class PaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
