using ArchiSubscription.Core.Enums;

namespace ArchiSubscription.Core.Entities;

/// <summary>
/// Represents a payment made for a specific subscription in a specific billing period.
/// </summary>
public class Payment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Period { get; set; } = string.Empty; // Format: "2026-05"
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }

    // Navigation property
    public Subscription Subscription { get; set; } = null!;
}

