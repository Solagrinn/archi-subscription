using ArchiSubscription.Core.Interfaces.Services;
using ArchiSubscription.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ArchiSubscription.Infrastructure.ExternalServices;

public class MockPaymentProcessingService : IPaymentProcessingService
{
    private readonly ILogger<MockPaymentProcessingService> _logger;
    private readonly TimeWarpService _timeWarp;
    private static readonly Random Random = new();

    public MockPaymentProcessingService(ILogger<MockPaymentProcessingService> logger, TimeWarpService timeWarp)
    {
        _logger = logger;
        _timeWarp = timeWarp;
    }

    public async Task<PaymentProcessingResult> ProcessAsync(
        decimal amount,
        string subscriberNumber,
        string serviceProvider,
        CancellationToken cancellationToken = default)
    {
        // Simulate network delay (100-500ms)
        await Task.Delay(Random.Next(100, 500), cancellationToken);

        _logger.LogInformation(
            "Mock Payment Processing: Processing {Amount:C} payment to {ServiceProvider} for subscriber {SubscriberNumber}",
            amount, serviceProvider, subscriberNumber);

        // 90% success rate
        var isSuccessful = Random.Next(1, 11) <= 9;
        var transactionRef = $"TXN-{_timeWarp.UtcNow:yyyyMMddHHmmss}-{Random.Next(10000, 99999)}";

        if (isSuccessful)
        {
            _logger.LogInformation("Mock Payment Processing: Payment successful. Reference: {TransactionRef}", transactionRef);
            return new PaymentProcessingResult(true, transactionRef, null);
        }

        _logger.LogWarning("Mock Payment Processing: Payment failed. Reference: {TransactionRef}", transactionRef);
        return new PaymentProcessingResult(false, transactionRef, "Payment declined by the payment gateway. Please try again.");
    }
}

