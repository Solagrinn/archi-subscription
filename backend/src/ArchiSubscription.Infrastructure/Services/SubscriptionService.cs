using ArchiSubscription.Core.DTOs.Subscription;
using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Enums;
using ArchiSubscription.Core.Exceptions;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Core.Interfaces.Services;

namespace ArchiSubscription.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly TimeWarpService _timeWarp;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        ICustomerRepository customerRepository,
        TimeWarpService timeWarp)
    {
        _subscriptionRepository = subscriptionRepository;
        _customerRepository = customerRepository;
        _timeWarp = timeWarp;
    }

    public async Task<SubscriptionResponseDto> CreateAsync(CreateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), dto.CustomerId);

        var subscription = new Subscription
        {
            CustomerId = dto.CustomerId,
            Type = dto.Type,
            ServiceProvider = dto.ServiceProvider,
            SubscriberNumber = dto.SubscriberNumber,
            PaymentDayOfMonth = dto.PaymentDayOfMonth,
            Status = SubscriptionStatus.Active,
            Customer = customer
        };

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        return MapToDto(subscription);
    }

    public async Task<SubscriptionResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdWithPaymentsAsync(id, cancellationToken);
        if (subscription is null)
            throw new NotFoundException(nameof(Subscription), id);

        return MapToDto(subscription);
    }

    public async Task<IReadOnlyList<SubscriptionResponseDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (!await _customerRepository.ExistsAsync(customerId, cancellationToken))
            throw new NotFoundException(nameof(Customer), customerId);

        var subscriptions = await _subscriptionRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return subscriptions.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<SubscriptionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);
        return subscriptions.Select(MapToDto).ToList();
    }

    public async Task<SubscriptionResponseDto> UpdateAsync(Guid id, UpdateSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdWithPaymentsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), id);

        if (dto.Type.HasValue) subscription.Type = dto.Type.Value;
        if (dto.ServiceProvider is not null) subscription.ServiceProvider = dto.ServiceProvider;
        if (dto.SubscriberNumber is not null) subscription.SubscriberNumber = dto.SubscriberNumber;
        if (dto.Status.HasValue) subscription.Status = dto.Status.Value;
        if (dto.PaymentDayOfMonth.HasValue) subscription.PaymentDayOfMonth = dto.PaymentDayOfMonth.Value;

        await _subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        return MapToDto(subscription);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), id);

        await _subscriptionRepository.DeleteAsync(subscription, cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionResponseDto>> GetUnpaidSubscriptionsForCurrentMonthAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (!await _customerRepository.ExistsAsync(customerId, cancellationToken))
            throw new NotFoundException(nameof(Customer), customerId);

        var now = _timeWarp.UtcNow;

        // Single query: load subscriptions WITH payments to avoid N+1
        var subscriptions = await _subscriptionRepository.GetByCustomerIdWithPaymentsAsync(customerId, cancellationToken);

        var active = subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();
        var result = new List<SubscriptionResponseDto>();

        foreach (var sub in active)
        {
            // Check months -3 to +1 for any unpaid period
            var hasUnpaid = false;
            for (var offset = -3; offset <= 1; offset++)
            {
                var refMonth = new DateTime(now.Year, now.Month, 1).AddMonths(offset);
                var period = refMonth.ToString("yyyy-MM");
                var payDate = GetPaymentDateForMonth(sub.PaymentDayOfMonth, refMonth);

                // Skip if subscription didn't exist yet
                if (sub.CreatedAt > payDate) continue;

                // For next month, only include if within 7 days
                if (offset == 1)
                {
                    var daysUntil = (payDate.Date - now.Date).Days;
                    if (daysUntil > 7) continue;
                }

                var isPaid = sub.Payments.Any(p => p.Period == period && p.Status == PaymentStatus.Successful);
                if (!isPaid)
                {
                    hasUnpaid = true;
                    break;
                }
            }

            if (hasUnpaid) result.Add(MapToDto(sub));
        }

        return result;
    }

    private static DateTime GetPaymentDateForMonth(int dayOfMonth, DateTime reference)
    {
        var daysInMonth = DateTime.DaysInMonth(reference.Year, reference.Month);
        var day = Math.Min(dayOfMonth, daysInMonth);
        return new DateTime(reference.Year, reference.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }

    private static SubscriptionResponseDto MapToDto(Subscription subscription)
    {
        return new SubscriptionResponseDto(
            subscription.Id,
            subscription.CustomerId,
            subscription.Customer?.FullName ?? string.Empty,
            subscription.Type,
            subscription.ServiceProvider,
            subscription.SubscriberNumber,
            subscription.Status,
            subscription.PaymentDayOfMonth,
            subscription.CreatedAt
        );
    }
}

