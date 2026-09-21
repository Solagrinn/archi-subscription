namespace ArchiSubscription.Core.DTOs.Customer;

public record CustomerResponseDto(
    Guid Id,
    string FullName,
    string Email,
    string PhoneNumber,
    DateTime CreatedAt,
    int ActiveSubscriptionCount
);

