using System.Collections.Concurrent;
using System.Diagnostics;
using Galaxxis.Application.Commands;
using Galaxxis.Domain.Buildings;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// Plays the synthetic players: gives commands at a steady rate through <see cref="ICommandService"/>, exactly as the
/// web application does, and follows every recorded command until the engine has decided, like the browser's status
/// polling (<c>commands.js</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Command mix.</b> 60 % "upgrade a random resource building", 40 % "set a random resource building to a random
/// speed between 50 and 100 %". A construction is refused right away while the planet's construction queue is busy,
/// which is the common case with 1000 players and about 30 construction requests per second; such refusals are counted
/// separately and cost the engine nothing.
/// </para>
/// <para>
/// <b>Steady rate.</b> Ten times per second a tenth of the rate is started, each command in its own task and DI scope
/// (one scope is one web request). At most <see cref="MaxInFlight"/> commands are underway at once; if the database is
/// so slow that this is reached, further commands are counted as <c>skipped</c> instead of piling up in memory.
/// </para>
/// </remarks>
internal sealed class PlayerSimulation(IServiceProvider services, LoadTestUniverse universe)
{
    /// <summary>Same interval as the browser's status polling in <c>commands.js</c>.</summary>
    private static readonly TimeSpan StatusPollInterval = TimeSpan.FromSeconds(1);

    /// <summary>A command not decided within this time counts as a timeout.</summary>
    private static readonly TimeSpan DecisionTimeout = TimeSpan.FromSeconds(60);

    private const int MaxInFlight = 2000;

    private readonly BuildingType[] _resourceBuildings = BuildingCatalog.ResourceBuildings.Select(rules => rules.Type).ToArray();
    private readonly ConcurrentQueue<double> _recordDurations = new();
    private readonly ConcurrentQueue<double> _decisionDurations = new();
    private readonly ConcurrentDictionary<string, int> _counts = new();
    private readonly ConcurrentDictionary<string, int> _errors = new();
    private int _inFlight;

    /// <summary>Commands given so far (all outcomes).</summary>
    public int Sent => Count("sent");

    /// <summary>Commands recorded for the engine so far.</summary>
    public int Recorded => Count("recorded");

    /// <summary>Gives commands at the given rate until cancelled, then waits for the commands still underway.</summary>
    /// <param name="commandsPerSecond">The rate.</param>
    /// <param name="cancellationToken">Ends the sending.</param>
    /// <returns>A task that completes when all started commands have finished.</returns>
    public async Task RunAsync(int commandsPerSecond, CancellationToken cancellationToken)
    {
        List<Task> running = [];
        double perSlice = commandsPerSecond / 10.0;
        double owed = 0;

        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(100));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                owed += perSlice;
                for (; owed >= 1; owed--)
                {
                    if (Volatile.Read(ref _inFlight) >= MaxInFlight)
                    {
                        Increment("skipped (client overloaded)");
                        continue;
                    }

                    Interlocked.Increment(ref _inFlight);
                    running.Add(Task.Run(() => GiveOneCommandAsync(CancellationToken.None), CancellationToken.None));
                }

                running.RemoveAll(task => task.IsCompleted);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal end of the sending phase.
        }

        // Commands already given are followed until decided, so the report covers all of them.
        await Task.WhenAll(running);
    }

    /// <summary>The results of all commands.</summary>
    /// <returns>A snapshot for the report.</returns>
    public SimulationResult Result() => new(
        new SortedDictionary<string, int>(_counts),
        new SortedDictionary<string, int>(_errors),
        Statistics.Of(_recordDurations),
        Statistics.Of(_decisionDurations));

    private async Task GiveOneCommandAsync(CancellationToken cancellationToken)
    {
        try
        {
            SimulatedPlayer player = universe.Players[Random.Shared.Next(universe.Players.Count)];
            BuildingType building = _resourceBuildings[Random.Shared.Next(_resourceBuildings.Length)];
            bool build = Random.Shared.NextDouble() < 0.6;

            await using AsyncServiceScope scope = services.CreateAsyncScope();
            ICommandService commands = scope.ServiceProvider.GetRequiredService<ICommandService>();

            Increment("sent");
            long start = Stopwatch.GetTimestamp();
            CommandResult result = build
                ? await commands.RequestBuildAsync(player.AccountId, player.PlanetId, building, cancellationToken)
                : await commands.RequestProductionSpeedAsync(player.AccountId, player.PlanetId, building, 50 + (10 * Random.Shared.Next(6)), cancellationToken);
            _recordDurations.Enqueue(Stopwatch.GetElapsedTime(start).TotalMilliseconds);

            switch (result.Outcome)
            {
                case CommandOutcome.Refused:
                    Increment($"refused immediately: {result.Message}");
                    return;
                case CommandOutcome.NotFound:
                    // Would mean the ownership check fails for a player's own planet.
                    Increment("not found (unexpected)");
                    return;
            }

            Increment("recorded");
            CommandStatusView? status = await FollowUntilDecidedAsync(commands, player.AccountId, result.EventId!.Value, start, cancellationToken);
            Increment(status is null ? "not decided within 60 s" : $"engine: {status.State}");
        }
        catch (Exception exception)
        {
            // Every error is counted by type, so a run with database trouble still produces a report.
            Increment("error");
            _errors.AddOrUpdate($"{exception.GetType().Name}: {exception.Message}", 1, (_, count) => count + 1);
        }
        finally
        {
            Interlocked.Decrement(ref _inFlight);
        }
    }

    /// <summary>Polls the status like the browser does; records the time until the engine decided.</summary>
    private async Task<CommandStatusView?> FollowUntilDecidedAsync(
        ICommandService commands, string accountId, long eventId, long start, CancellationToken cancellationToken)
    {
        while (Stopwatch.GetElapsedTime(start) < DecisionTimeout)
        {
            await Task.Delay(StatusPollInterval, cancellationToken);

            CommandStatusView? status = await commands.GetStatusAsync(accountId, eventId, cancellationToken);
            if (status is { IsDecided: true })
            {
                _decisionDurations.Enqueue(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
                return status;
            }

            if (status is null)
            {
                throw new InvalidOperationException($"Command {eventId} of {accountId} disappeared.");
            }
        }

        return null;
    }

    private void Increment(string key) => _counts.AddOrUpdate(key, 1, (_, count) => count + 1);

    private int Count(string key) => _counts.GetValueOrDefault(key);
}

/// <summary>What became of the simulated commands.</summary>
/// <param name="Counts">Number of commands per outcome, e.g. "recorded", "engine: Scheduled", "refused immediately: …".</param>
/// <param name="Errors">Exceptions by type and message.</param>
/// <param name="RecordDurations">Time to record a command (the web request), in milliseconds.</param>
/// <param name="DecisionDurations">
/// Time from giving a command until the status poll saw the engine's decision, in milliseconds. Polling once per second,
/// like the browser, so the values are rounded up to whole seconds plus the poll's own time.
/// </param>
internal sealed record SimulationResult(
    IReadOnlyDictionary<string, int> Counts,
    IReadOnlyDictionary<string, int> Errors,
    Statistics RecordDurations,
    Statistics DecisionDurations);
