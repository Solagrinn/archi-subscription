using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.DTOs.Payment;

public record PaymentResponseDto(
    Guid Id,
    Guid SubscriptionId,
    string ServiceProvider,
    string SubscriberNumber,
    decimal Amount,
    DateTime PaymentDate,
    string Period,
    PaymentStatus Status,
    string? TransactionReference
);

