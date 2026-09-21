using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.DTOs.Subscription;

public record CreateSubscriptionDto(
    Guid CustomerId,
    SubscriptionType Type,
    string ServiceProvider,
    string SubscriberNumber,
    int PaymentDayOfMonth
);

