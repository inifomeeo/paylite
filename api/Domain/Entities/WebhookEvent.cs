using api.Domain.Enums;

namespace api.Domain.Entities;

public class WebhookEvent
{
    public Guid Id { get; set; }
    public string EventExternalId { get; set; } = string.Empty;
    public Guid? PaymentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public bool Verified { get; set; }
    public PaymentStatus? Status { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ProcessedAtUtc { get; set; }

    public Payment? Payment { get; set; }
}
