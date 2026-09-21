using ArchiSubscription.Core.DTOs.Subscription;
using ArchiSubscription.Core.Interfaces.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ArchiSubscription.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IDebtInquiryService _debtInquiryService;
    private readonly IValidator<CreateSubscriptionDto> _createValidator;
    private readonly IValidator<UpdateSubscriptionDto> _updateValidator;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        IDebtInquiryService debtInquiryService,
        IValidator<CreateSubscriptionDto> createValidator,
        IValidator<UpdateSubscriptionDto> updateValidator)
    {
        _subscriptionService = subscriptionService;
        _debtInquiryService = debtInquiryService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Get all active subscriptions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionService.GetAllAsync(cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Get a subscription by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.GetByIdAsync(id, cancellationToken);
        return Ok(subscription);
    }

    /// <summary>
    /// Get all subscriptions for a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionService.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Get unpaid subscriptions for the current month for a specific customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}/unpaid")]
    [ProducesResponseType(typeof(IReadOnlyList<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUnpaidForCurrentMonth(Guid customerId, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionService.GetUnpaidSubscriptionsForCurrentMonthAsync(customerId, cancellationToken);
        return Ok(subscriptions);
    }

    /// <summary>
    /// Create a new subscription.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var subscription = await _subscriptionService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, subscription);
    }

    /// <summary>
    /// Update an existing subscription.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionDto dto, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

        var subscription = await _subscriptionService.UpdateAsync(id, dto, cancellationToken);
        return Ok(subscription);
    }

    /// <summary>
    /// Delete a subscription.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _subscriptionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Inquire debt for a subscription from the third-party service provider.
    /// Optionally specify a billing period (e.g. "2026-03") to check a specific month.
    /// </summary>
    [HttpGet("{id:guid}/debt")]
    [ProducesResponseType(typeof(Core.DTOs.DebtInquiry.DebtInquiryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InquireDebt(Guid id, [FromQuery] string? period, CancellationToken cancellationToken)
    {
        var debtInfo = await _debtInquiryService.InquireDebtAsync(id, period, cancellationToken);
        return Ok(debtInfo);
    }
}

