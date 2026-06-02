namespace api.Domain.Entities;

public class IdempotencyKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Payment Payment { get; set; } = null!;
}
