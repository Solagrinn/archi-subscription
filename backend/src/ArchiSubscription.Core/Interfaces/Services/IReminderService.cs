using ArchiSubscription.Core.DTOs.Reminder;

namespace ArchiSubscription.Core.Interfaces.Services;

/// <summary>
/// Service to check subscriptions due for payment and generate reminders.
/// </summary>
public interface IReminderService
{
    Task<IReadOnlyList<ReminderResponseDto>> GetPendingRemindersAsync(Guid? customerId = null, int daysAhead = 3, CancellationToken cancellationToken = default);
}

