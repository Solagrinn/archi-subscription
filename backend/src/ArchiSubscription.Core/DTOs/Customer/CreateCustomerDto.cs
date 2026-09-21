namespace ArchiSubscription.Core.DTOs.Customer;

public record CreateCustomerDto(
    string FullName,
    string Email,
    string PhoneNumber
);

