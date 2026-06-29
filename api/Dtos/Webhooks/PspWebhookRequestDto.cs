using System.ComponentModel.DataAnnotations;
using api.Domain.Enums;

namespace api.Dtos.Webhooks;

public class PspWebhookRequestDto
{
    [Required]
    [StringLength(100)]
    public string EventExternalId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public Guid PaymentId { get; set; }

    [Required]
    [EnumDataType(typeof(PaymentStatus))]
    public PaymentStatus Status { get; set; }

    [StringLength(250)]
    public string FailureReason { get; set; } = string.Empty;
}
