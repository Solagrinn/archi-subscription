using ArchiSubscription.Core.DTOs.Reminder;
using ArchiSubscription.Core.Interfaces.Services;
using ArchiSubscription.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArchiSubscription.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;
    private readonly INotificationService _notificationService;
    private readonly TimeWarpService _timeWarp;

    public RemindersController(
        IReminderService reminderService,
        INotificationService notificationService,
        TimeWarpService timeWarp)
    {
        _reminderService = reminderService;
        _notificationService = notificationService;
        _timeWarp = timeWarp;
    }

    /// <summary>
    /// Get pending payment reminders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReminderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReminders(
        [FromQuery] Guid? customerId,
        [FromQuery] int daysAhead = 3,
        CancellationToken cancellationToken = default)
    {
        var reminders = await _reminderService.GetPendingRemindersAsync(customerId, daysAhead, cancellationToken);
        return Ok(new
        {
            count = reminders.Count,
            checkedAt = _timeWarp.UtcNow,
            reminders
        });
    }

    /// <summary>
    /// Send notification (mock email + SMS) for all pending reminders of a customer.
    /// </summary>
    [HttpPost("notify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendNotifications(
        [FromQuery] Guid customerId,
        [FromQuery] int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        var reminders = await _reminderService.GetPendingRemindersAsync(customerId, daysAhead, cancellationToken);
        var results = new List<object>();

        foreach (var r in reminders)
        {
            var subject = $"Payment Reminder: {r.ServiceProvider} ({r.SubscriptionType})";
            var body = $"Dear {r.CustomerName},\n\n{r.Message}\n\nSubscriber No: {r.SubscriberNumber}\nPeriod: {r.Period}\nPayment day: {r.PaymentDayOfMonth}\n\nPlease make your payment on time.";

            var emailResult = await _notificationService.SendEmailAsync(r.CustomerEmail, subject, body, cancellationToken);
            var smsResult = await _notificationService.SendSmsAsync(r.CustomerPhone, r.Message, cancellationToken);

            results.Add(new
            {
                subscriptionId = r.SubscriptionId,
                serviceProvider = r.ServiceProvider,
                email = new { emailResult.Success, emailResult.Channel, emailResult.Recipient },
                sms = new { smsResult.Success, smsResult.Channel, smsResult.Recipient }
            });
        }

        return Ok(new { notificationsSent = results.Count, results });
    }
}

