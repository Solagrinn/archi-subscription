using ArchiSubscription.Core.Entities;

namespace ArchiSubscription.Core.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllWithSubscriptionsAsync(CancellationToken cancellationToken = default);
}

