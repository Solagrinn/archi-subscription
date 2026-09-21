using ArchiSubscription.Core.DTOs.Subscription;

namespace ArchiSubscription.Core.Interfaces.Services;

public interface ISubscriptionService
{
    Task<SubscriptionResponseDto> CreateAsync(CreateSubscriptionDto dto, CancellationToken cancellationToken = default);
    Task<SubscriptionResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionResponseDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionResponseDto> UpdateAsync(Guid id, UpdateSubscriptionDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionResponseDto>> GetUnpaidSubscriptionsForCurrentMonthAsync(Guid customerId, CancellationToken cancellationToken = default);
}

