namespace ArchiSubscription.Core.Entities;

/// <summary>
/// Represents a bank customer who can have multiple subscriptions.
/// </summary>
public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

