using ArchiSubscription.Core.DTOs.DebtInquiry;
using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Exceptions;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Core.Interfaces.Services;
using ArchiSubscription.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ArchiSubscription.Infrastructure.ExternalServices;

public class MockDebtInquiryService : IDebtInquiryService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<MockDebtInquiryService> _logger;
    private readonly TimeWarpService _timeWarp;
    private static readonly Random Random = new();

    public MockDebtInquiryService(
        ISubscriptionRepository subscriptionRepository,
        ILogger<MockDebtInquiryService> logger,
        TimeWarpService timeWarp)
    {
        _subscriptionRepository = subscriptionRepository;
        _logger = logger;
        _timeWarp = timeWarp;
    }

    public async Task<DebtInquiryResponseDto> InquireDebtAsync(Guid subscriptionId, string? period = null, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdWithPaymentsAsync(subscriptionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), subscriptionId);

        // Simulate network delay (50-200ms)
        await Task.Delay(Random.Next(50, 200), cancellationToken);

        _logger.LogInformation(
            "Mock Debt Inquiry: Querying {ServiceProvider} for subscriber {SubscriberNumber} (period: {Period})",
            subscription.ServiceProvider,
            subscription.SubscriberNumber,
            period ?? "auto");

        // Generate mock debt based on subscription type
        var baseAmount = subscription.Type switch
        {
            Core.Enums.SubscriptionType.Electricity => Random.Next(80, 350),
            Core.Enums.SubscriptionType.Water => Random.Next(30, 150),
            Core.Enums.SubscriptionType.Internet => Random.Next(100, 300),
            Core.Enums.SubscriptionType.GSM => Random.Next(50, 200),
            Core.Enums.SubscriptionType.NaturalGas => Random.Next(60, 400),
            Core.Enums.SubscriptionType.Insurance => Random.Next(200, 800),
            _ => Random.Next(50, 250)
        };

        var now = _timeWarp.UtcNow;

        // If a specific period is requested, use it. Otherwise, find the oldest unpaid period.
        string debtPeriod;
        DateTime dueDate;

        if (!string.IsNullOrEmpty(period))
        {
            // Specific period requested (e.g. "2026-02")
            debtPeriod = period;
            var parts = period.Split('-');
            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);
            var refDate = new DateTime(year, month, 1);
            dueDate = GetPaymentDateForMonth(subscription.PaymentDayOfMonth, refDate);
        }
        else
        {
            // Auto-detect: find the oldest unpaid period from month-3 to month+1
            debtPeriod = now.ToString("yyyy-MM");
            dueDate = GetPaymentDateForMonth(subscription.PaymentDayOfMonth, now);

            for (var offset = -3; offset <= 1; offset++)
            {
                var refMonth = new DateTime(now.Year, now.Month, 1).AddMonths(offset);
                var candidatePeriod = refMonth.ToString("yyyy-MM");
                var isPaid = subscription.Payments?.Any(p =>
                    p.Period == candidatePeriod &&
                    p.Status == Core.Enums.PaymentStatus.Successful) ?? false;

                if (!isPaid)
                {
                    // Only include next month if within 7 days
                    if (offset == 1)
                    {
                        var nextDue = GetPaymentDateForMonth(subscription.PaymentDayOfMonth, refMonth);
                        if ((nextDue - now.Date).Days > 7) continue;
                    }

                    debtPeriod = candidatePeriod;
                    dueDate = GetPaymentDateForMonth(subscription.PaymentDayOfMonth, refMonth);
                    break;
                }
            }
        }

        var hasPaid = subscription.Payments?.Any(p =>
            p.Period == debtPeriod &&
            p.Status == Core.Enums.PaymentStatus.Successful) ?? false;

        return new DebtInquiryResponseDto(
            subscriptionId,
            subscription.ServiceProvider,
            subscription.SubscriberNumber,
            DebtAmount: hasPaid ? 0m : baseAmount + Random.Next(0, 99) / 100m,
            DueDate: dueDate,
            Period: debtPeriod,
            HasDebt: !hasPaid
        );
    }

    private static DateTime GetPaymentDateForMonth(int dayOfMonth, DateTime reference)
    {
        var daysInMonth = DateTime.DaysInMonth(reference.Year, reference.Month);
        var day = Math.Min(dayOfMonth, daysInMonth);
        return new DateTime(reference.Year, reference.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}

