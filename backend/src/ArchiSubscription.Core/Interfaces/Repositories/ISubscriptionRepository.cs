using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.Interfaces.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<IReadOnlyList<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetByCustomerIdWithPaymentsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<Subscription?> GetByIdWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task CancelOverdueAsync(IEnumerable<Guid> subscriptionIds, DateTime updatedAt, CancellationToken cancellationToken = default);
}

