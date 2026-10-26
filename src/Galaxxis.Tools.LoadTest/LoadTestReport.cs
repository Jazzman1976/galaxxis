using System.Globalization;
using System.Text;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// Everything a load test run measured, and its rendering as a Markdown report (printed to the console and, with
/// <c>--report</c>, written to a file for <c>docs/audits/</c>).
/// </summary>
/// <param name="Settings">What was simulated.</param>
/// <param name="UniverseId">The load test universe.</param>
/// <param name="Environment">Where it ran: machine, database server, engine settings.</param>
/// <param name="StartedAt">When sending commands started.</param>
/// <param name="SendingDuration">How long commands were actually sent (shorter if stopped with Ctrl+C).</param>
/// <param name="Simulation">What became of the commands.</param>
/// <param name="EventDurations">Engine: processing time per event (empty if the engine ran elsewhere).</param>
/// <param name="CycleDurations">Engine: duration per cycle.</param>
/// <param name="Backlog">The backlog samples taken during the run.</param>
/// <param name="CatchUp">How long the engine needed after the last command until nothing was due any more.</param>
/// <param name="EventsByTypeAndState">All events of the universe at the end, grouped.</param>
/// <param name="Checks">The correctness checks.</param>
internal sealed record LoadTestReport(
    LoadTestSettings Settings,
    int UniverseId,
    IReadOnlyDictionary<string, string> Environment,
    DateTimeOffset StartedAt,
    TimeSpan SendingDuration,
    SimulationResult Simulation,
    Statistics EventDurations,
    Statistics CycleDurations,
    IReadOnlyList<BacklogSample> Backlog,
    TimeSpan? CatchUp,
    IReadOnlyList<(string Type, string State, int Count)> EventsByTypeAndState,
    IReadOnlyList<CheckResult> Checks)
{
    /// <summary>
    /// Acceptance limit for the processing time per event: "im einstelligen Millisekundenbereich" (plan, "Verifikation").
    /// The median and the 95th percentile must stay below it.
    /// </summary>
    public const double EventDurationLimitMs = 10;

    /// <summary>
    /// Acceptance limit for the backlog: in 95 % of the samples the oldest due event is at most this old. One poll
    /// interval plus one tick is normal; more means events pile up.
    /// </summary>
    public static readonly TimeSpan BacklogLagLimit = TimeSpan.FromSeconds(3);

    /// <summary>Whether all acceptance criteria are met.</summary>
    public bool Accepted => Criteria().All(criterion => criterion.Met);

    /// <summary>Renders the report as Markdown.</summary>
    /// <returns>The report text.</returns>
    public string ToMarkdown()
    {
        StringBuilder text = new();
        CultureInfo invariant = CultureInfo.InvariantCulture;

        text.AppendLine(invariant, $"# Galaxxis load test {StartedAt.ToLocalTime():yyyy-MM-dd HH:mm}");
        text.AppendLine();
        text.AppendLine(invariant, $"**Result: {(Accepted ? "ACCEPTED" : "NOT ACCEPTED")}** (universe {UniverseId})");
        text.AppendLine();

        text.AppendLine("## Acceptance criteria");
        text.AppendLine();
        text.AppendLine("| Criterion | Measured | Met |");
        text.AppendLine("| --- | --- | --- |");
        foreach ((string name, string measured, bool met) in Criteria())
        {
            text.AppendLine(invariant, $"| {name} | {measured} | {(met ? "yes" : "**no**")} |");
        }

        text.AppendLine();
        text.AppendLine("## Settings and environment");
        text.AppendLine();
        text.AppendLine("| Setting | Value |");
        text.AppendLine("| --- | --- |");
        text.AppendLine(invariant, $"| Players | {Settings.Players} |");
        text.AppendLine(invariant, $"| Commands per second | {Settings.CommandsPerSecond} |");
        text.AppendLine(invariant, $"| Duration (planned / actual) | {Settings.Duration:hh\\:mm\\:ss} / {SendingDuration:hh\\:mm\\:ss} |");
        text.AppendLine(invariant, $"| Stale planets | {Settings.StalePercent} %, {Settings.StaleDays} day(s) behind |");
        text.AppendLine(invariant, $"| Tick length | {Settings.TickLength} |");
        foreach ((string key, string value) in Environment)
        {
            text.AppendLine(invariant, $"| {key} | {value} |");
        }

        text.AppendLine();
        text.AppendLine("## Commands (players' view)");
        text.AppendLine();
        text.AppendLine("| Outcome | Count |");
        text.AppendLine("| --- | ---: |");
        foreach ((string outcome, int count) in Simulation.Counts)
        {
            text.AppendLine(invariant, $"| {outcome} | {count} |");
        }

        text.AppendLine();
        AppendStatisticsTable(text, "Timings (milliseconds)",
        [
            ("Record a command (web request)", Simulation.RecordDurations),
            ("Command until decision seen (1 s status polling)", Simulation.DecisionDurations),
            ("Engine: one event", EventDurations),
            ("Engine: one cycle", CycleDurations),
        ]);

        if (EventDurations.Count == 0)
        {
            text.AppendLine();
            text.AppendLine("The engine did not run in the load test process (Engine:Enabled = false), so there are no engine timings.");
        }

        text.AppendLine();
        text.AppendLine("## Backlog (sampled every second)");
        text.AppendLine();
        Statistics dueCounts = Statistics.Of(Backlog.Select(sample => (double)sample.DueEvents));
        Statistics lags = Statistics.Of(Backlog.Select(sample => sample.OldestLag.TotalSeconds));
        double throughput = SendingDuration > TimeSpan.Zero ? EventDurations.Count / (SendingDuration + (CatchUp ?? TimeSpan.Zero)).TotalSeconds : 0;
        text.AppendLine("| | Median | 95 % | Max |");
        text.AppendLine("| --- | ---: | ---: | ---: |");
        text.AppendLine(invariant, $"| Due events | {dueCounts.Median:0} | {dueCounts.P95:0} | {dueCounts.Max:0} |");
        text.AppendLine(invariant, $"| Lag of the oldest due event (s) | {lags.Median:0} | {lags.P95:0} | {lags.Max:0} |");
        text.AppendLine();
        text.AppendLine(invariant, $"Samples: {Backlog.Count}. Engine throughput: {throughput:0.0} events per second on average. "
            + $"Catch-up after the last command: {(CatchUp is TimeSpan catchUp ? $"{catchUp.TotalSeconds:0.0} s" : "not reached")}.");

        text.AppendLine();
        text.AppendLine("## Events in the database at the end");
        text.AppendLine();
        text.AppendLine("| Type | State | Count |");
        text.AppendLine("| --- | --- | ---: |");
        foreach ((string type, string state, int count) in EventsByTypeAndState)
        {
            text.AppendLine(invariant, $"| {type} | {state} | {count} |");
        }

        text.AppendLine();
        AppendChecks(text, Checks);

        if (Simulation.Errors.Count > 0)
        {
            text.AppendLine();
            text.AppendLine("## Errors");
            text.AppendLine();
            foreach ((string error, int count) in Simulation.Errors)
            {
                text.AppendLine(invariant, $"- {count}x {error}");
            }
        }

        return text.ToString();
    }

    /// <summary>Appends the correctness checks as a Markdown section.</summary>
    /// <param name="text">The report being written.</param>
    /// <param name="checks">The check results.</param>
    public static void AppendChecks(StringBuilder text, IReadOnlyList<CheckResult> checks)
    {
        text.AppendLine("## Correctness checks");
        text.AppendLine();
        text.AppendLine("| Check | Looked at | Passed |");
        text.AppendLine("| --- | --- | --- |");
        foreach (CheckResult check in checks)
        {
            text.AppendLine(CultureInfo.InvariantCulture, $"| {check.Name} | {check.Scope} | {(check.Passed ? "yes" : "**no**")} |");
        }

        foreach (CheckResult check in checks.Where(check => !check.Passed))
        {
            text.AppendLine();
            text.AppendLine(CultureInfo.InvariantCulture, $"{check.Name}:");
            foreach (string problem in check.Problems)
            {
                text.AppendLine(CultureInfo.InvariantCulture, $"- {problem}");
            }
        }
    }

    /// <summary>The acceptance criteria of the plan with the measured values.</summary>
    private IEnumerable<(string Name, string Measured, bool Met)> Criteria()
    {
        Statistics lags = Statistics.Of(Backlog.Select(sample => sample.OldestLag.TotalSeconds));
        yield return (
            $"Backlog stays near 0: oldest due event at most {BacklogLagLimit.TotalSeconds:0} s old in 95 % of the samples",
            string.Create(CultureInfo.InvariantCulture, $"95 %: {lags.P95:0} s, max: {lags.Max:0} s"),
            Backlog.Count > 0 && lags.P95 <= BacklogLagLimit.TotalSeconds);

        if (EventDurations.Count > 0)
        {
            yield return (
                $"Processing time per event in the single-digit milliseconds (median and 95 % below {EventDurationLimitMs:0} ms)",
                string.Create(CultureInfo.InvariantCulture, $"median {EventDurations.Median:0.0} ms, 95 %: {EventDurations.P95:0.0} ms"),
                EventDurations.Median < EventDurationLimitMs && EventDurations.P95 < EventDurationLimitMs);
        }

        yield return (
            "All correctness checks pass (no doubled levels, no negative stock, ...)",
            $"{Checks.Count(check => check.Passed)} of {Checks.Count} passed",
            Checks.All(check => check.Passed));

        int errors = Simulation.Errors.Values.Sum();
        yield return ("No errors while giving commands", $"{errors} error(s)", errors == 0);
    }

    private static void AppendStatisticsTable(StringBuilder text, string title, IReadOnlyList<(string Name, Statistics Values)> rows)
    {
        text.AppendLine(CultureInfo.InvariantCulture, $"## {title}");
        text.AppendLine();
        text.AppendLine("| | Count | Average | Median | 95 % | 99 % | Max |");
        text.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach ((string name, Statistics values) in rows)
        {
            text.AppendLine(CultureInfo.InvariantCulture,
                $"| {name} | {values.Count} | {values.Average:0.0} | {values.Median:0.0} | {values.P95:0.0} | {values.P99:0.0} | {values.Max:0.0} |");
        }
    }
}
