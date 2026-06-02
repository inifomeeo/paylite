using System.ComponentModel.DataAnnotations.Schema;
using api.Domain.Enums;

namespace api.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<IdempotencyKey> IdempotencyKeys { get; set; } = [];
    public ICollection<WebhookEvent> WebhookEvents { get; set; } = [];
}
