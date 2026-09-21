using ArchiSubscription.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArchiSubscription.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TimeController : ControllerBase
{
    private readonly TimeWarpService _timeWarp;

    public TimeController(TimeWarpService timeWarp)
    {
        _timeWarp = timeWarp;
    }

    /// <summary>
    /// Get the current virtual backend date/time.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TimeInfoResponse), StatusCodes.Status200OK)]
    public IActionResult GetCurrentTime()
    {
        return Ok(new TimeInfoResponse(_timeWarp.UtcNow, _timeWarp.OffsetDays));
    }

    /// <summary>
    /// Advance virtual time by 1 day.
    /// </summary>
    [HttpPost("forward")]
    [ProducesResponseType(typeof(TimeInfoResponse), StatusCodes.Status200OK)]
    public IActionResult Forward()
    {
        var now = _timeWarp.Forward();
        return Ok(new TimeInfoResponse(now, _timeWarp.OffsetDays));
    }

    /// <summary>
    /// Rewind virtual time by 1 day.
    /// </summary>
    [HttpPost("backward")]
    [ProducesResponseType(typeof(TimeInfoResponse), StatusCodes.Status200OK)]
    public IActionResult Backward()
    {
        var now = _timeWarp.Backward();
        return Ok(new TimeInfoResponse(now, _timeWarp.OffsetDays));
    }

    /// <summary>
    /// Reset virtual time to real time.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(TimeInfoResponse), StatusCodes.Status200OK)]
    public IActionResult Reset()
    {
        var now = _timeWarp.Reset();
        return Ok(new TimeInfoResponse(now, _timeWarp.OffsetDays));
    }
}

public record TimeInfoResponse(DateTime CurrentDate, int OffsetDays);

