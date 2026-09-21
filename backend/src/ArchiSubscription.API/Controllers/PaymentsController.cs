using ArchiSubscription.Core.DTOs.Payment;
using ArchiSubscription.Core.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ArchiSubscription.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IValidator<CreatePaymentDto> _validator;

    public PaymentsController(IPaymentService paymentService, IValidator<CreatePaymentDto> validator)
    {
        _paymentService = paymentService;
        _validator = validator;
    }

    /// <summary>
    /// Process a payment for a subscription.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var payment = await _paymentService.ProcessPaymentAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    /// <summary>
    /// Get a payment by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetByIdAsync(id, cancellationToken);
        return Ok(payment);
    }

    /// <summary>
    /// Get all payments for a specific subscription.
    /// </summary>
    [HttpGet("subscription/{subscriptionId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySubscriptionId(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var payments = await _paymentService.GetBySubscriptionIdAsync(subscriptionId, cancellationToken);
        return Ok(payments);
    }

    /// <summary>
    /// Get all payments for a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        var payments = await _paymentService.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(payments);
    }
}

