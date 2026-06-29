using api.Dtos.Webhooks;

namespace api.Interfaces;

public interface IWebhookService
{
    Task<WebhookProcessResponseDto> ProcessAsync(string payload, string signature, CancellationToken cancellationToken);
}
