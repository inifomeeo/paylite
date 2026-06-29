using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using api.Data;
using api.Domain.Entities;
using api.Dtos.Payment;
using api.Interfaces;
using api.Options;
using api.Exceptions;

namespace api.Services;

public class PaymentService(
    ApplicationDBContext dbContext,
    IIdempotencyService idempotencyService,
    IOptions<PaymentOptions> paymentOptions) : IPaymentService
{
    private readonly ApplicationDBContext _dbContext = dbContext;
    private readonly IIdempotencyService _idempotencyService = idempotencyService;
    private readonly PaymentOptions _paymentOptions = paymentOptions.Value;

    public async Task<PaymentDto> CreateAsync(
        CreatePaymentRequestDto request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ValidationException("The Idempotency-Key header is required.");
        }

        if (_paymentOptions.SupportedCurrencies.Length > 0 &&
            !_paymentOptions.SupportedCurrencies.Contains(request.Currency, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException($"Unsupported currency '{request.Currency}'.");
        }

        var requestHash = _idempotencyService.ComputeRequestHash(request);
        var existingPayment = await _idempotencyService.GetPaymentAsync(idempotencyKey, requestHash, cancellationToken);
        if (existingPayment is not null)
        {
            return MapToPaymentDto(existingPayment);
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = request.Currency,
            Reference = request.Reference,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _idempotencyService.StorePaymentAsync(idempotencyKey, requestHash, payment.Id, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return MapToPaymentDto(payment);
    }

    public async Task<PaymentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException($"Payment {id} was not found.");
        }
        
        return MapToPaymentDto(payment);
    }

    private static PaymentDto MapToPaymentDto(Payment payment) => new()
    {
        
        Id = payment.Id,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Reference = payment.Reference,
        Status = payment.Status,
        FailureReason = payment.FailureReason,
        CreatedAtUtc = payment.CreatedAtUtc,
        UpdatedAtUtc = payment.UpdatedAtUtc
    };
}
