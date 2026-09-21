using ArchiSubscription.Core.Entities;

namespace ArchiSubscription.Core.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<Payment?> GetBySubscriptionAndPeriodAsync(Guid subscriptionId, string period, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdWithSubscriptionAsync(Guid id, CancellationToken cancellationToken = default);
}

