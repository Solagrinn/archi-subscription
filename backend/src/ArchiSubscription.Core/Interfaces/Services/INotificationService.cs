namespace ArchiSubscription.Core.Interfaces.Services;

/// <summary>
/// Service for sending notifications (email, SMS) to customers.
/// </summary>
public interface INotificationService
{
    Task<NotificationResult> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task<NotificationResult> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}

public record NotificationResult(bool Success, string Channel, string Recipient, string? ErrorMessage = null);

