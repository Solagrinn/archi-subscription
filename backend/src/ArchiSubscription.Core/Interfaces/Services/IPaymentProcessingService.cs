namespace ArchiSubscription.Core.Interfaces.Services;

/// <summary>
/// Third-party mock service for processing payments.
/// </summary>
public interface IPaymentProcessingService
{
    Task<PaymentProcessingResult> ProcessAsync(decimal amount, string subscriberNumber, string serviceProvider, CancellationToken cancellationToken = default);
}

public record PaymentProcessingResult(
    bool IsSuccessful,
    string TransactionReference,
    string? ErrorMessage
);

