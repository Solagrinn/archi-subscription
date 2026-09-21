using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.DTOs.Reminder;

/// <summary>
/// Represents a reminder for an unpaid subscription approaching its due date.
/// </summary>
public record ReminderResponseDto(
    Guid SubscriptionId,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    SubscriptionType SubscriptionType,
    string ServiceProvider,
    string SubscriberNumber,
    int PaymentDayOfMonth,
    string Period,
    string Message
);

