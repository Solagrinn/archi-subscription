using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Enums;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiSubscription.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Payment>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(p => p.Subscription)
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetBySubscriptionAndPeriodAsync(Guid subscriptionId, string period, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.SubscriptionId == subscriptionId &&
                p.Period == period &&
                p.Status == PaymentStatus.Successful,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(p => p.Subscription)
            .Where(p => p.Subscription.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByIdWithSubscriptionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}

