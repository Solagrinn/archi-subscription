using ArchiSubscription.Core.DTOs.Reminder;
using ArchiSubscription.Core.Enums;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Core.Interfaces.Services;

namespace ArchiSubscription.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly TimeWarpService _timeWarp;

    public ReminderService(
        ISubscriptionRepository subscriptionRepository,
        TimeWarpService timeWarp)
    {
        _subscriptionRepository = subscriptionRepository;
        _timeWarp = timeWarp;
    }

    public async Task<IReadOnlyList<ReminderResponseDto>> GetPendingRemindersAsync(
        Guid? customerId = null,
        int daysAhead = 3,
        CancellationToken cancellationToken = default)
    {
        var today = _timeWarp.UtcNow.Date;

        // Single query: GetActiveSubscriptionsAsync already includes .Include(s => s.Payments)
        var activeSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);

        if (customerId.HasValue)
        {
            activeSubscriptions = activeSubscriptions
                .Where(s => s.CustomerId == customerId.Value)
                .ToList();
        }

        // ── Generate reminders (read-only — no side effects) ──
        var reminders = new List<ReminderResponseDto>();

        foreach (var subscription in activeSubscriptions)
        {
            // Check billing periods from month-3 to month+1
            for (var offset = -3; offset <= 1; offset++)
            {
                var refMonth = new DateTime(today.Year, today.Month, 1).AddMonths(offset);
                var period = refMonth.ToString("yyyy-MM");
                var paymentDate = GetPaymentDateForMonth(subscription.PaymentDayOfMonth, refMonth);
                var daysUntilPayment = (paymentDate - today).Days;

                // Only check period if subscription existed by then
                if (subscription.CreatedAt > paymentDate) continue;

                // Skip if too far in the future
                if (daysUntilPayment > daysAhead) continue;

                // Check if already paid for this specific period (in-memory)
                var alreadyPaid = subscription.Payments?.Any(p =>
                    p.Period == period &&
                    p.Status == PaymentStatus.Successful) ?? false;

                if (alreadyPaid) continue;

                var message = daysUntilPayment switch
                {
                    < -90 => $"🚨 CRITICAL: Payment {Math.Abs(daysUntilPayment)} days overdue! Was due {paymentDate:MMM d} ({period}). Subscription at risk of cancellation.",
                    < 0 => $"⚠️ Payment overdue! Was due on {paymentDate:MMM d} ({period}).",
                    0 => $"🔔 Payment is due TODAY for {subscription.ServiceProvider} ({period}).",
                    1 => $"📅 Payment due TOMORROW for {subscription.ServiceProvider} ({period}).",
                    _ => $"📅 Payment due in {daysUntilPayment} day(s) for {subscription.ServiceProvider} ({period})."
                };

                reminders.Add(new ReminderResponseDto(
                    subscription.Id,
                    subscription.CustomerId,
                    subscription.Customer?.FullName ?? string.Empty,
                    subscription.Customer?.Email ?? string.Empty,
                    subscription.Customer?.PhoneNumber ?? string.Empty,
                    subscription.Type,
                    subscription.ServiceProvider,
                    subscription.SubscriberNumber,
                    subscription.PaymentDayOfMonth,
                    period,
                    message
                ));
            }
        }

        return reminders.OrderBy(r => r.Period).ThenBy(r => r.PaymentDayOfMonth).ToList();
    }

    private static DateTime GetPaymentDateForMonth(int dayOfMonth, DateTime reference)
    {
        var daysInMonth = DateTime.DaysInMonth(reference.Year, reference.Month);
        var day = Math.Min(dayOfMonth, daysInMonth);
        return new DateTime(reference.Year, reference.Month, day, 0, 0, 0, DateTimeKind.Utc);
    }
}

