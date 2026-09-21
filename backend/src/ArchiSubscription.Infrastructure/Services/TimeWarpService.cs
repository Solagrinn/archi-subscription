namespace ArchiSubscription.Infrastructure.Services;

/// <summary>
/// Singleton service that provides a virtual "current time" for the application.
/// Allows time-warping forward/backward for testing purposes.
/// </summary>
public class TimeWarpService
{
    private int _offsetDays;

    /// <summary>
    /// Gets the current virtual UTC time (real UTC + offset).
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow.AddDays(_offsetDays);

    /// <summary>
    /// Gets the current offset in days.
    /// </summary>
    public int OffsetDays => _offsetDays;

    /// <summary>
    /// Advance virtual time by 1 day.
    /// </summary>
    public DateTime Forward()
    {
        Interlocked.Increment(ref _offsetDays);
        return UtcNow;
    }

    /// <summary>
    /// Rewind virtual time by 1 day.
    /// </summary>
    public DateTime Backward()
    {
        Interlocked.Decrement(ref _offsetDays);
        return UtcNow;
    }

    /// <summary>
    /// Reset to real time.
    /// </summary>
    public DateTime Reset()
    {
        Interlocked.Exchange(ref _offsetDays, 0);
        return UtcNow;
    }
}

