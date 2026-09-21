using ArchiSubscription.Core.DTOs.Payment;

namespace ArchiSubscription.Core.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponseDto>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentResponseDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<PaymentResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

