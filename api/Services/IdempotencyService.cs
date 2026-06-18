using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using api.Data;
using api.Domain.Entities;
using api.Dtos.Payment;
using api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class IdempotencyService(ApplicationDBContext dbContext) : IIdempotencyService
{
    private readonly ApplicationDBContext _dbContext = dbContext;
    public string ComputeRequestHash(CreatePaymentRequestDto request)
    {
        var serialized = JsonSerializer.Serialize(new
        {
            request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            request.Reference
        });

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(serialized));
        return Convert.ToHexString(hashBytes);
    }

    public async Task<Payment?> GetPaymentAsync(string key, string requestHash, CancellationToken cancellationToken)
    {
        var record = await _dbContext.IdempotencyKeys
            .Include(x => x.Payment)
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken);

        if (record is null)
        {
            return null;
        }

        return record.Payment;
    }

    public async Task StorePaymentAsync(string key, string requestHash, Guid paymentId, CancellationToken cancellationToken)
    {
        if (key.Length > 100)
        {
            throw new ValidationException("The idempotency key header must be 100 characters or fewer.");
        }

        _dbContext.IdempotencyKeys.Add(new IdempotencyKey
        {
            Id = Guid.NewGuid(),
            Key = key,
            RequestHash = requestHash,
            PaymentId = paymentId,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
