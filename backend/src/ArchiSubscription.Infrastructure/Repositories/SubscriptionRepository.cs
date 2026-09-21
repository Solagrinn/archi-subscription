using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Enums;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiSubscription.Infrastructure.Repositories;

public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(s => s.Customer)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByCustomerIdWithPaymentsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Payments)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Payments)
            .Where(s => s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<Subscription?> GetByIdWithPaymentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Customer)
            .Include(s => s.Payments.OrderByDescending(p => p.PaymentDate))
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task CancelOverdueAsync(IEnumerable<Guid> subscriptionIds, DateTime updatedAt, CancellationToken cancellationToken = default)
    {
        var ids = subscriptionIds.ToList();
        if (ids.Count == 0) return;

        // Single SQL UPDATE — no entity loading, no N+1
        await DbSet
            .Where(s => ids.Contains(s.Id) && s.Status == SubscriptionStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, SubscriptionStatus.Passive)
                .SetProperty(x => x.UpdatedAt, updatedAt),
                cancellationToken);
    }
}

