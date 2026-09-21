using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.Entities;

/// <summary>
/// Represents a recurring subscription (e.g., electricity, water, internet, GSM).
/// A subscription is defined once and does not need to be recreated each month.
/// </summary>
public class Subscription : BaseEntity
{
    public Guid CustomerId { get; set; }
    public SubscriptionType Type { get; set; }
    public string ServiceProvider { get; set; } = string.Empty;
    public string SubscriberNumber { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public int PaymentDayOfMonth { get; set; } = 1;

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

