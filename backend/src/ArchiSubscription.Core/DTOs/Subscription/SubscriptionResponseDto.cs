using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.DTOs.Subscription;

public record SubscriptionResponseDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    SubscriptionType Type,
    string ServiceProvider,
    string SubscriberNumber,
    SubscriptionStatus Status,
    int PaymentDayOfMonth,
    DateTime CreatedAt
);

