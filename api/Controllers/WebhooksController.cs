using System.Text;
using api.Dtos.Webhooks;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/webhooks/psp")]
public class WebhooksController(IWebhookService webhookService) : ControllerBase
{
    private readonly IWebhookService _webhookService = webhookService;

    [HttpPost]
    [ProducesResponseType(typeof(WebhookProcessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ProcessPspWebhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();

        string payload;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
        {
            payload = await reader.ReadToEndAsync(cancellationToken);
        }

        Request.Body.Position = 0;

        var signature = Request.Headers["X-PSP-Signature"].ToString();
        var response = await _webhookService.ProcessAsync(payload, signature, cancellationToken);
        return Ok(response);
    }        
}
