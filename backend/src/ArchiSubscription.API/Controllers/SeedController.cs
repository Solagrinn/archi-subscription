using ArchiSubscription.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArchiSubscription.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SeedController : ControllerBase
{
    private readonly DataSeederService _seeder;

    public SeedController(DataSeederService seeder)
    {
        _seeder = seeder;
    }

    /// <summary>
    /// Seed the database with a new customer, subscriptions, and payments.
    /// Press multiple times to add more customers with test data.
    /// Each call creates 1 customer, 4-6 subscriptions, and payments.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SeedResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        var result = await _seeder.SeedAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Clear ALL data from the database (customers, subscriptions, payments).
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        var message = await _seeder.ClearAsync(cancellationToken);
        return Ok(new { message });
    }
}

