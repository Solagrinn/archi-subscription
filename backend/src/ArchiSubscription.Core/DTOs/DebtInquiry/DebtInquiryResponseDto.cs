namespace ArchiSubscription.Core.DTOs.DebtInquiry;

/// <summary>
/// Response from the third-party debt inquiry service.
/// </summary>
public record DebtInquiryResponseDto(
    Guid SubscriptionId,
    string ServiceProvider,
    string SubscriberNumber,
    decimal DebtAmount,
    DateTime DueDate,
    string Period,
    bool HasDebt
);

