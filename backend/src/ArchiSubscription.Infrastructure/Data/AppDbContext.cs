using ArchiSubscription.Core.Entities;
using ArchiSubscription.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ArchiSubscription.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly TimeWarpService _timeWarp;

    public AppDbContext(DbContextOptions<AppDbContext> options, TimeWarpService timeWarp) : base(options)
    {
        _timeWarp = timeWarp;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeWarp.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Only set CreatedAt if not explicitly set by the caller
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
            }
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

