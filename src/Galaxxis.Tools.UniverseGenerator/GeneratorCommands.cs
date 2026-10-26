using System.Globalization;
using Galaxxis.Application.Accounts;
using Galaxxis.Application.Universes;
using Galaxxis.Domain.Universes.Generation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Galaxxis.Tools.UniverseGenerator;

/// <summary>
/// The commands of the universe generator tool. Each command runs in its own dependency injection scope and
/// prints its result to the console.
/// </summary>
internal static class GeneratorCommands
{
    /// <summary>The help text shown by "help" and after invalid input.</summary>
    public const string Usage = """
        Galaxxis universe generator

        Commands:
          generate [options]      Generate a new universe. It starts paused.
              --name <text>           Name (default: random)
              --seed <number>         Random seed; the same seed generates the same map (default: random)
              --galaxies <number>     Number of galaxies (default from configuration)
              --systems <number>      Star systems per galaxy (default from configuration)
              --planets <number>      Planets per star system (default from configuration)
              --tick-length <time>    Real time per tick, e.g. 00:00:01 (default from configuration)
          list                    List all universes.
          resume <id>             Start or continue universe time.
          pause <id>              Stop universe time, e.g. for maintenance.
          bootstrap-admin         Create the admin account from the BootstrapAdmin settings.
          help                    Show this text.

        Without a command, an interactive menu starts.
        """;

    /// <summary>Runs the command given on the command line.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <param name="args">The command line arguments; the first one is the command.</param>
    /// <returns>The process exit code: 0 on success, 1 on error.</returns>
    public static async Task<int> RunAsync(IServiceProvider services, string[] args)
    {
        try
        {
            CommandArguments arguments = CommandArguments.Parse(args.Skip(1));

            switch (args[0].ToLowerInvariant())
            {
                case "generate":
                    await GenerateAsync(services, BuildRequest(services, arguments));
                    return 0;
                case "list":
                    await ListAsync(services);
                    return 0;
                case "resume":
                    return await ResumeAsync(services, arguments.RequireId()) ? 0 : 1;
                case "pause":
                    return await PauseAsync(services, arguments.RequireId()) ? 0 : 1;
                case "bootstrap-admin":
                    return await BootstrapAdminAsync(services) ? 0 : 1;
                case "help":
                    Console.WriteLine(Usage);
                    return 0;
                default:
                    Console.Error.WriteLine($"Unknown command \"{args[0]}\".");
                    Console.WriteLine(Usage);
                    return 1;
            }
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or InvalidOperationException)
        {
            // Mistakes in the input or settings: a readable message is more helpful than a stack trace.
            Console.Error.WriteLine($"Error: {exception.Message}");
            return 1;
        }
    }

    /// <summary>Generates a universe and prints its summary.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <param name="request">What to generate.</param>
    /// <returns>A task that completes when the universe is stored.</returns>
    public static async Task GenerateAsync(IServiceProvider services, GenerateUniverseRequest request)
    {
        int planets = request.Map.GalaxyCount * request.Map.StarSystemsPerGalaxy * request.Map.PlanetsPerStarSystem;
        Console.WriteLine($"Generating {request.Map.GalaxyCount} galaxies with {planets} planets in total ...");

        using IServiceScope scope = services.CreateScope();
        UniverseSummary universe = await scope.ServiceProvider.GetRequiredService<IUniverseAdminService>().GenerateAsync(request);

        Console.WriteLine();
        Print(universe);
        Console.WriteLine();
        Console.WriteLine($"The universe is paused. Start it with: resume {universe.Id}");
    }

    /// <summary>Prints all universes.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <returns>A task that completes when the list is printed.</returns>
    public static async Task ListAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        IReadOnlyList<UniverseSummary> universes = await scope.ServiceProvider.GetRequiredService<IUniverseAdminService>().ListAsync();

        if (universes.Count == 0)
        {
            Console.WriteLine("No universes yet. Create one with: generate");
            return;
        }

        foreach (UniverseSummary universe in universes)
        {
            Print(universe);
        }
    }

    /// <summary>Resumes a universe and prints the result.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <param name="universeId">The universe to resume.</param>
    /// <returns><see langword="true"/> if the universe exists.</returns>
    public static async Task<bool> ResumeAsync(IServiceProvider services, int universeId)
    {
        using IServiceScope scope = services.CreateScope();
        UniverseSummary? universe = await scope.ServiceProvider.GetRequiredService<IUniverseAdminService>().ResumeAsync(universeId);
        return PrintOrReportMissing(universe, universeId);
    }

    /// <summary>Pauses a universe and prints the result.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <param name="universeId">The universe to pause.</param>
    /// <returns><see langword="true"/> if the universe exists.</returns>
    public static async Task<bool> PauseAsync(IServiceProvider services, int universeId)
    {
        using IServiceScope scope = services.CreateScope();
        UniverseSummary? universe = await scope.ServiceProvider.GetRequiredService<IUniverseAdminService>().PauseAsync(universeId);
        return PrintOrReportMissing(universe, universeId);
    }

    /// <summary>Creates the admin account from the configuration.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <returns><see langword="true"/> on success.</returns>
    public static async Task<bool> BootstrapAdminAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        try
        {
            AdminBootstrapResult result = await scope.ServiceProvider.GetRequiredService<IAdminAccountBootstrapper>().EnsureAdminAsync();
            Console.WriteLine(result == AdminBootstrapResult.Created
                ? "The admin account has been created."
                : "The admin account already existed; it has the admin role.");
            return true;
        }
        catch (OptionsValidationException exception)
        {
            foreach (string failure in exception.Failures)
            {
                Console.Error.WriteLine($"Error: {failure}");
            }

            return false;
        }
    }

    /// <summary>Builds a generation request from the configured defaults and the given overrides.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <param name="arguments">Command line options; missing options keep the defaults.</param>
    /// <returns>The request.</returns>
    public static GenerateUniverseRequest BuildRequest(IServiceProvider services, CommandArguments arguments)
    {
        UniverseGenerationOptions defaults = services.GetRequiredService<IOptions<UniverseGenerationOptions>>().Value;

        UniverseGenerationSettings map = defaults.Map with
        {
            Name = arguments.Text("name") ?? defaults.Map.Name,
            GalaxyCount = arguments.Number("galaxies") ?? defaults.Map.GalaxyCount,
            StarSystemsPerGalaxy = arguments.Number("systems") ?? defaults.Map.StarSystemsPerGalaxy,
            PlanetsPerStarSystem = arguments.Number("planets") ?? defaults.Map.PlanetsPerStarSystem,
        };

        return new GenerateUniverseRequest(map, arguments.Number("seed"), arguments.Duration("tick-length"));
    }

    private static bool PrintOrReportMissing(UniverseSummary? universe, int universeId)
    {
        if (universe is null)
        {
            Console.Error.WriteLine($"Error: there is no universe with id {universeId}. Use \"list\" to see all universes.");
            return false;
        }

        Print(universe);
        return true;
    }

    private static void Print(UniverseSummary universe) =>
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"#{universe.Id} {universe.Name}: {(universe.IsPaused ? "paused" : "running")}, " +
            $"{universe.GalaxyCount} galaxies, {universe.PlanetCount} planets ({universe.ClaimedPlanetCount} claimed), " +
            $"tick {universe.TickLength}, seed {universe.Seed}, created {universe.CreatedAt:yyyy-MM-dd HH:mm} UTC"));
}
