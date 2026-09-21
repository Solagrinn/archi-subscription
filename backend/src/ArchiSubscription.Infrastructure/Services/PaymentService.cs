using ArchiSubscription.Core.DTOs.Payment;
using ArchiSubscription.Core.Entities;
using ArchiSubscription.Core.Enums;
using ArchiSubscription.Core.Exceptions;
using ArchiSubscription.Core.Interfaces.Repositories;
using ArchiSubscription.Core.Interfaces.Services;

namespace ArchiSubscription.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentProcessingService _paymentProcessingService;
    private readonly TimeWarpService _timeWarp;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ISubscriptionRepository subscriptionRepository,
        IPaymentProcessingService paymentProcessingService,
        TimeWarpService timeWarp)
    {
        _paymentRepository = paymentRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentProcessingService = paymentProcessingService;
        _timeWarp = timeWarp;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        var subscription = await _subscriptionRepository.GetByIdWithPaymentsAsync(dto.SubscriptionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Subscription), dto.SubscriptionId);

        if (subscription.Status != SubscriptionStatus.Active)
            throw new BadRequestException("Cannot make payment for an inactive subscription.");

        // Check if already paid in-memory (payments loaded via Include)
        var alreadyPaid = subscription.Payments?.Any(p =>
            p.Period == dto.Period &&
            p.Status == PaymentStatus.Successful) ?? false;
        if (alreadyPaid)
            throw new BadRequestException($"Payment for period '{dto.Period}' has already been made for this subscription.");

        // Call mock payment processing service
        var processingResult = await _paymentProcessingService.ProcessAsync(
            dto.Amount,
            subscription.SubscriberNumber,
            subscription.ServiceProvider,
            cancellationToken);

        var payment = new Payment
        {
            SubscriptionId = dto.SubscriptionId,
            Amount = dto.Amount,
            PaymentDate = _timeWarp.UtcNow,
            Period = dto.Period,
            Status = processingResult.IsSuccessful ? PaymentStatus.Successful : PaymentStatus.Failed,
            TransactionReference = processingResult.TransactionReference
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        return new PaymentResponseDto(
            payment.Id,
            payment.SubscriptionId,
            subscription.ServiceProvider,
            subscription.SubscriberNumber,
            payment.Amount,
            payment.PaymentDate,
            payment.Period,
            payment.Status,
            payment.TransactionReference
        );
    }

    public async Task<IReadOnlyList<PaymentResponseDto>> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetBySubscriptionIdAsync(subscriptionId, cancellationToken);
        return payments.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<PaymentResponseDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return payments.Select(MapToDto).ToList();
    }

    public async Task<PaymentResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Single query with Include — no N+1
        var payment = await _paymentRepository.GetByIdWithSubscriptionAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Payment), id);

        return MapToDto(payment);
    }

    private static PaymentResponseDto MapToDto(Payment payment)
    {
        return new PaymentResponseDto(
            payment.Id,
            payment.SubscriptionId,
            payment.Subscription?.ServiceProvider ?? string.Empty,
            payment.Subscription?.SubscriberNumber ?? string.Empty,
            payment.Amount,
            payment.PaymentDate,
            payment.Period,
            payment.Status,
            payment.TransactionReference
        );
    }
}

