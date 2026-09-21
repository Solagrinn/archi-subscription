using ArchiSubscription.Core.DTOs.DebtInquiry;

namespace ArchiSubscription.Core.Interfaces.Services;

/// <summary>
/// Third-party mock service for querying debt information from service providers.
/// </summary>
public interface IDebtInquiryService
{
    Task<DebtInquiryResponseDto> InquireDebtAsync(Guid subscriptionId, string? period = null, CancellationToken cancellationToken = default);
}

