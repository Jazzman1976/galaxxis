using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Galaxxis.Application.Universes;
using Galaxxis.Engine.Configuration;
using Galaxxis.Persistence.Configuration;
using Galaxxis.Persistence.Game;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// The commands of the load test tool: <c>run</c> (build a universe, simulate players, measure, check) and
/// <c>verify</c> (let the engine catch up on an existing load test universe and check it, e.g. after a hard restart).
/// </summary>
internal static class LoadTestCommands
{
    /// <summary>The help text shown by "help" and after invalid input.</summary>
    public const string Usage = """
        Galaxxis load test

        Commands:
          run [options]           Generate a load test universe, let synthetic players give commands, measure the
                                  engine and check the game state afterwards. Ctrl+C ends the sending early.
              --players <number>      Synthetic players (default 1000)
              --rate <number>         Commands per second of all players together (default 50)
              --duration <time>       How long commands are sent, e.g. 00:30:00 (default 00:30:00)
              --stale-percent <n>     Share of planets not settled for days, 0-100 (default 20)
              --stale-days <n>        How many days those planets lag behind (default 3)
              --tick-length <time>    Real time per tick (default 00:00:01)
              --report <file>         Also write the Markdown report to this file
          verify <id>             Let the engine process everything due in load test universe <id>, then run the
                                  correctness checks. Used after a hard restart (failure drill).
          help                    Show this text.

        Use separate databases for load tests (see README.md): set ConnectionStrings__Game and
        ConnectionStrings__Accounts as environment variables before starting.
        """;

    /// <summary>How long <c>run</c> waits after the last command for the engine to process everything due.</summary>
    private static readonly TimeSpan RunCatchUpLimit = TimeSpan.FromMinutes(2);

    /// <summary>How long <c>verify</c> waits for the engine; after a long outage there can be a lot to do.</summary>
    private static readonly TimeSpan VerifyCatchUpLimit = TimeSpan.FromMinutes(15);

    /// <summary>Runs the command given on the command line.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <param name="args">The command line arguments; the first one is the command.</param>
    /// <returns>The process exit code: 0 on success, 1 on error or failed acceptance.</returns>
    public static async Task<int> RunAsync(IServiceProvider services, string[] args)
    {
        try
        {
            CommandArguments arguments = CommandArguments.Parse(args.Skip(1));

            switch (args[0].ToLowerInvariant())
            {
                case "run":
                    return await RunLoadTestAsync(services, ReadSettings(arguments), arguments.Text("report"));
                case "verify":
                    return await VerifyAsync(services, arguments.RequireId());
                case "help":
                    Console.WriteLine(Usage);
                    return 0;
                default:
                    Console.Error.WriteLine($"Unknown command \"{args[0]}\".");
                    Console.WriteLine(Usage);
                    return 1;
            }
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException)
        {
            // Mistakes in the input: a readable message is more helpful than a stack trace.
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 1;
        }
    }

    private static LoadTestSettings ReadSettings(CommandArguments arguments)
    {
        LoadTestSettings defaults = LoadTestSettings.Acceptance;
        LoadTestSettings settings = new(
            arguments.Number("players") ?? defaults.Players,
            arguments.Number("rate") ?? defaults.CommandsPerSecond,
            arguments.Duration("duration") ?? defaults.Duration,
            arguments.Number("stale-percent") ?? defaults.StalePercent,
            arguments.Number("stale-days") ?? defaults.StaleDays,
            arguments.Duration("tick-length") ?? defaults.TickLength);

        settings.Validate();
        return settings;
    }

    /// <summary>The <c>run</c> command.</summary>
    private static async Task<int> RunLoadTestAsync(IServiceProvider services, LoadTestSettings settings, string? reportPath)
    {
        // Ctrl+C ends the sending phase early; the run still catches up, checks and reports.
        using CancellationTokenSource sending = new();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Console.WriteLine("Stopping: no more commands are sent; the run catches up, checks and reports.");
            sending.Cancel();
        };

        using EngineMeasurements engine = new();
        IDbContextFactory<GameDbContext> contextFactory = services.GetRequiredService<IDbContextFactory<GameDbContext>>();

        LoadTestUniverse universe = await LoadTestWorld.CreateAsync(services, settings, CancellationToken.None);
        BacklogMonitor backlog = new(contextFactory, universe.UniverseId);
        PlayerSimulation simulation = new(services, universe);

        Console.WriteLine($"Sending {settings.CommandsPerSecond} commands per second for {settings.Duration} ...");
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        Stopwatch sendingClock = Stopwatch.StartNew();
        sending.CancelAfter(settings.Duration);

        using CancellationTokenSource monitoring = new();
        Task backlogTask = backlog.RunAsync(monitoring.Token);
        Task progressTask = PrintProgressAsync(sendingClock, simulation, engine, backlog, sending.Token);

        await simulation.RunAsync(settings.CommandsPerSecond, sending.Token);
        TimeSpan sendingDuration = sendingClock.Elapsed;
        await progressTask;

        Console.WriteLine("All commands are decided. Waiting until the engine has nothing due any more ...");
        TimeSpan? catchUp = await WaitUntilNothingDueAsync(backlog, RunCatchUpLimit);

        await monitoring.CancelAsync();
        await backlogTask;

        await using GameDbContext database = await contextFactory.CreateDbContextAsync();
        IReadOnlyList<CheckResult> checks = await InvariantChecks.RunAsync(database, universe.UniverseId, CancellationToken.None);
        IReadOnlyList<(string Type, string State, int Count)> events = await EventsByTypeAndStateAsync(database, universe.UniverseId);

        LoadTestReport report = new(
            settings,
            universe.UniverseId,
            DescribeEnvironment(services),
            startedAt,
            sendingDuration,
            simulation.Result(),
            Statistics.Of(engine.EventDurations),
            Statistics.Of(engine.CycleDurations),
            backlog.Samples,
            catchUp,
            events,
            checks);

        // The load test universe is paused, so its remaining constructions do not keep an engine busy later.
        using (IServiceScope scope = services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IUniverseAdminService>().PauseAsync(universe.UniverseId);
        }

        string markdown = report.ToMarkdown();
        Console.WriteLine();
        Console.WriteLine(markdown);

        if (reportPath is not null)
        {
            await File.WriteAllTextAsync(reportPath, markdown, Encoding.UTF8);
            Console.WriteLine($"Report written to {Path.GetFullPath(reportPath)}.");
        }

        Console.WriteLine($"Universe {universe.UniverseId} is paused now.");
        return report.Accepted ? 0 : 1;
    }

    /// <summary>The <c>verify</c> command.</summary>
    private static async Task<int> VerifyAsync(IServiceProvider services, int universeId)
    {
        IDbContextFactory<GameDbContext> contextFactory = services.GetRequiredService<IDbContextFactory<GameDbContext>>();
        await using GameDbContext database = await contextFactory.CreateDbContextAsync();

        if (await database.Universes.AsNoTracking().SingleOrDefaultAsync(u => u.Id == universeId) is not { } universe)
        {
            Console.Error.WriteLine($"There is no universe with id {universeId}.");
            return 1;
        }

        if (universe.IsPaused)
        {
            Console.WriteLine($"Universe {universeId} is paused; nothing becomes due. Checking the current state only.");
        }

        BacklogMonitor backlog = new(contextFactory, universeId);
        BacklogSample before = await backlog.SampleAsync(CancellationToken.None);
        Console.WriteLine($"Due now: {before.DueEvents} event(s), the oldest {before.OldestLag.TotalSeconds:0} s behind. Waiting for the engine ...");

        TimeSpan? catchUp = universe.IsPaused ? TimeSpan.Zero : await WaitUntilNothingDueAsync(backlog, VerifyCatchUpLimit);
        Console.WriteLine(catchUp is TimeSpan time
            ? $"Nothing due any more after {time.TotalSeconds:0.0} s."
            : $"Still events due after {VerifyCatchUpLimit}; is the engine running (Engine:Enabled)?");

        IReadOnlyList<CheckResult> checks = await InvariantChecks.RunAsync(database, universeId, CancellationToken.None);
        StringBuilder text = new();
        LoadTestReport.AppendChecks(text, checks);
        Console.WriteLine();
        Console.WriteLine(text);

        return catchUp is not null && checks.All(check => check.Passed) ? 0 : 1;
    }

    /// <summary>Waits until the backlog sample shows no due event, polling every second.</summary>
    /// <returns>How long it took, or <see langword="null"/> if the limit was reached first.</returns>
    private static async Task<TimeSpan?> WaitUntilNothingDueAsync(BacklogMonitor backlog, TimeSpan limit)
    {
        Stopwatch clock = Stopwatch.StartNew();
        while (clock.Elapsed < limit)
        {
            BacklogSample sample = await backlog.SampleAsync(CancellationToken.None);
            if (sample.DueEvents == 0)
            {
                return clock.Elapsed;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        return null;
    }

    /// <summary>Prints one progress line every ten seconds while commands are sent.</summary>
    private static async Task PrintProgressAsync(
        Stopwatch clock, PlayerSimulation simulation, EngineMeasurements engine, BacklogMonitor backlog, CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                Statistics events = Statistics.Of(engine.EventDurations);
                BacklogSample? latest = backlog.Latest;
                Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
                    $"{clock.Elapsed:hh\\:mm\\:ss} | sent {simulation.Sent} | recorded {simulation.Recorded} | " +
                    $"engine {events.Count} events, median {events.Median:0.0} ms, 95 % {events.P95:0.0} ms | " +
                    $"due now {latest?.DueEvents ?? 0}, oldest {latest?.OldestLag.TotalSeconds ?? 0:0} s"));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Sending has ended.
        }
    }

    private static async Task<IReadOnlyList<(string Type, string State, int Count)>> EventsByTypeAndStateAsync(GameDbContext database, int universeId)
    {
        var groups = await database.GameEvents
            .Where(e => e.UniverseId == universeId)
            .GroupBy(e => new { e.Type, e.State })
            .Select(group => new { group.Key.Type, group.Key.State, Count = group.Count() })
            .ToListAsync();

        return groups
            .OrderBy(group => group.Type, StringComparer.Ordinal)
            .ThenBy(group => group.State)
            .Select(group => (group.Type, group.State.ToString(), group.Count))
            .ToList();
    }

    /// <summary>Where the test ran, for the report. Contains no secrets: only the database server name.</summary>
    private static IReadOnlyDictionary<string, string> DescribeEnvironment(IServiceProvider services)
    {
        EngineOptions engine = services.GetRequiredService<IOptions<EngineOptions>>().Value;
        SqlConnectionStringBuilder game = new(services.GetRequiredService<IOptions<DatabaseConnectionOptions>>().Value.Game);

        return new Dictionary<string, string>
        {
            ["Machine"] = $"{RuntimeInformation.OSDescription}, {Environment.ProcessorCount} logical processors",
            [".NET"] = RuntimeInformation.FrameworkDescription,
            ["Database server"] = game.DataSource,
            ["Engine"] = engine.Enabled
                ? $"in this process, poll interval {engine.PollInterval}, at most {engine.MaxEventsPerUniversePerCycle} events per universe and cycle"
                : "in another process (Engine:Enabled = false here)",
            ["Build"] = typeof(LoadTestCommands).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
                .OfType<System.Reflection.AssemblyConfigurationAttribute>().FirstOrDefault()?.Configuration ?? "unknown",
        };
    }
}
