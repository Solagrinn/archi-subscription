using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiSubscription.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByIdWithSubscriptionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllWithSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(c => c.Subscriptions)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

