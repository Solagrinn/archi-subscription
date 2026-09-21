using ArchiSubscription.Core.DTOs.Customer;
using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Enums;
using ArchiSubscription.Core.Exceptions;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Core.Interfaces.Services;

namespace ArchiSubscription.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerResponseDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        return MapToDto(customer);
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithSubscriptionsAsync(id, cancellationToken);
        if (customer is null)
            throw new NotFoundException(nameof(Customer), id);

        return MapToDto(customer);
    }

    public async Task<IReadOnlyList<CustomerResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllWithSubscriptionsAsync(cancellationToken);
        return customers.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);

        await _customerRepository.DeleteAsync(customer, cancellationToken);
    }

    private static CustomerResponseDto MapToDto(Customer customer)
    {
        return new CustomerResponseDto(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.PhoneNumber,
            customer.CreatedAt,
            customer.Subscriptions?.Count(s => s.Status == SubscriptionStatus.Active) ?? 0
        );
    }
}

