using ArchiSubscription.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace ArchiSubscription.Infrastructure.ExternalServices;

/// <summary>
/// Mock implementation that logs email and SMS notifications instead of sending them.
/// </summary>
public class MockNotificationService : INotificationService
{
    private readonly ILogger<MockNotificationService> _logger;

    public MockNotificationService(ILogger<MockNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task<NotificationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Simulate network delay
        await Task.Delay(Random.Shared.Next(50, 150), cancellationToken);

        _logger.LogInformation(
            "📧 MOCK EMAIL SENT\n" +
            "   To:      {To}\n" +
            "   Subject: {Subject}\n" +
            "   Body:    {Body}",
            to, subject, body);

        return new NotificationResult(true, "Email", to);
    }

    public async Task<NotificationResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        // Simulate network delay
        await Task.Delay(Random.Shared.Next(50, 150), cancellationToken);

        _logger.LogInformation(
            "📱 MOCK SMS SENT\n" +
            "   To:      {PhoneNumber}\n" +
            "   Message: {Message}",
            phoneNumber, message);

        return new NotificationResult(true, "SMS", phoneNumber);
    }
}

