using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using api.Data;
using api.Domain.Entities;
using api.Dtos.Webhooks;
using api.Exceptions;
using api.Interfaces;
using api.Options;

namespace api.Services;

public class WebhookService(
    ApplicationDBContext dBContext,
    IOptions<SecurityOptions> securityOptions) : IWebhookService
{
    private const string SignatureHeaderName = "X-PSP-Signature";

    private readonly ApplicationDBContext _dbContext = dBContext;
    private readonly SecurityOptions _securityOptions = securityOptions.Value;

    public async Task<WebhookProcessResponseDto> ProcessAsync(string payload, string signature, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            throw new UnauthorizedException($"{SignatureHeaderName} header is missing.");
        }

        var request = JsonSerializer.Deserialize<PspWebhookRequestDto>(payload, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        }) ?? throw new ValidationException("Webhook payload is required.");

        Validator.ValidateObject(request, new ValidationContext(request), validateAllProperties: true);

        var existingEvent = await _dbContext.WebhookEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.EventExternalId == request.EventExternalId, cancellationToken);

        if (existingEvent is not null)
        {
            return new WebhookProcessResponseDto
            {
                EventExternalId = existingEvent.EventExternalId,
                Result = "Duplicate webhook ignored."
            };
        }

        var isVerified = VerifySignature(payload, signature);

        var webhookEvent = new WebhookEvent
        {
            Id = Guid.NewGuid(),
            EventExternalId = request.EventExternalId,
            EventType = request.EventType,
            Signature = signature,
            Payload = payload,
            Verified = isVerified,
            Status = request.Status,
            ReceivedAtUtc = DateTime.UtcNow
        };

        var payment = await _dbContext.Payments.SingleOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken);
        if (payment is null)
        {
            _dbContext.WebhookEvents.Add(webhookEvent);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new NotFoundException($"Payment with ID '{request.PaymentId}' referenced by webhook not found.");
        }

        webhookEvent.PaymentId = payment.Id;

        if (!isVerified)
        {
            _dbContext.WebhookEvents.Add(webhookEvent);
            await _dbContext.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException("Webhook signature verification failed.");
        }

        payment.Status = request.Status;
        payment.FailureReason = request.FailureReason;
        payment.UpdatedAtUtc = DateTime.UtcNow;
        webhookEvent.ProcessedAtUtc = DateTime.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.WebhookEvents.Add(webhookEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new WebhookProcessResponseDto
        {
            EventExternalId = webhookEvent.EventExternalId,
            Result = "Webhook processed successfully."
        };
    }

    private bool VerifySignature(string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(_securityOptions.WebhookSecret))
        {
            throw new UnauthorizedException("Webhook secret is not configured.");
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_securityOptions.WebhookSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToHexString(computedHash);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(signature.ToUpperInvariant()));
    }
}
