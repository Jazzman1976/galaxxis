namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// What one load test run simulates. Read from the command line of <c>run</c>; the defaults are the acceptance test
/// of the foundation refactoring (plan, section "Verifikation", item "Last").
/// </summary>
/// <param name="Players">Number of synthetic players; each joins the universe and gets one home planet.</param>
/// <param name="CommandsPerSecond">How many commands all players together give per second.</param>
/// <param name="Duration">How long commands are sent.</param>
/// <param name="StalePercent">
/// Share of planets (0–100) whose resources have not been settled for <paramref name="StaleDays"/> days, as if their
/// owners had not looked at them. The first event on such a planet settles days of production at once.
/// </param>
/// <param name="StaleDays">How many days the stale planets lag behind.</param>
/// <param name="TickLength">Real time per tick of the load test universe.</param>
internal sealed record LoadTestSettings(
    int Players,
    int CommandsPerSecond,
    TimeSpan Duration,
    int StalePercent,
    int StaleDays,
    TimeSpan TickLength)
{
    /// <summary>The acceptance values: 1000 players, 50 commands per second, 30 minutes, 20 % stale by 3 days.</summary>
    public static LoadTestSettings Acceptance { get; } = new(
        Players: 1000,
        CommandsPerSecond: 50,
        Duration: TimeSpan.FromMinutes(30),
        StalePercent: 20,
        StaleDays: 3,
        TickLength: TimeSpan.FromSeconds(1));

    /// <summary>Checks the values and explains what is wrong.</summary>
    /// <exception cref="ArgumentException">A value is out of range.</exception>
    public void Validate()
    {
        if (Players < 1)
        {
            throw new ArgumentException("--players must be at least 1.");
        }

        if (CommandsPerSecond is < 1 or > 10_000)
        {
            throw new ArgumentException("--rate must be between 1 and 10000 commands per second.");
        }

        if (Duration < TimeSpan.FromSeconds(10))
        {
            throw new ArgumentException("--duration must be at least 00:00:10.");
        }

        if (StalePercent is < 0 or > 100)
        {
            throw new ArgumentException("--stale-percent must be between 0 and 100.");
        }

        if (StaleDays is < 0 or > 365)
        {
            throw new ArgumentException("--stale-days must be between 0 and 365.");
        }
    }
}
