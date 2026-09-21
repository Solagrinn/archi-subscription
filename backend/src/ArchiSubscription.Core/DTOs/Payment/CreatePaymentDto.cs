namespace ArchiSubscription.Core.DTOs.Payment;

public record CreatePaymentDto(
    Guid SubscriptionId,
    decimal Amount,
    string Period // Format: "2026-05"
);

