namespace api.Dtos.Webhooks;

public class WebhookProcessResponseDto
{
    public string EventExternalId { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}
