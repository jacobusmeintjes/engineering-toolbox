namespace TestUtilities;

/// <summary>
/// Fake system clock for time-based testing (enables deterministic time in tests)
/// </summary>
public class FakeSystemClock
{
    private DateTime _currentTime = DateTime.UtcNow;

    public DateTime UtcNow => _currentTime;

    public DateOnly Today => DateOnly.FromDateTime(_currentTime);

    public void SetTime(DateTime time)
    {
        _currentTime = time;
    }

    public void AdvanceBy(TimeSpan duration)
    {
        _currentTime = _currentTime.Add(duration);
    }

    public void AdvanceDays(int days)
    {
        _currentTime = _currentTime.AddDays(days);
    }
}
