using api.Dtos.Payment;

namespace api.Interfaces;

public interface IPaymentService
{
    Task<PaymentDto> CreateAsync(CreatePaymentRequestDto request, string idempotencyKey, CancellationToken cancellationToken);
    Task<PaymentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
