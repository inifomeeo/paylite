using api.Domain.Entities;
using api.Dtos.Payment;

namespace api.Interfaces;

public interface IIdempotencyService
{
    string ComputeRequestHash(CreatePaymentRequestDto request);
    Task<Payment?> GetPaymentAsync(string key, string requestHash, CancellationToken cancellationToken);
    Task StorePaymentAsync(string key, string requestHash, Guid paymentId, CancellationToken cancellationToken);
}
