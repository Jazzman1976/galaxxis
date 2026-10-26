// Galaxxis load test: console entry point.
//
// Simulates many players (target: 1000) giving commands at the same time and measures how the engine copes:
// processing time per event, backlog of due events, and correctness checks such as "no building was upgraded twice"
// and "no resource stock went negative". The engine runs inside this process (as in the web application), so its own
// measurements can be collected; with Engine:Enabled = false another process (e.g. the web application) processes.
//
// Usage: see README.md, or run with "help".

using System.Globalization;
using Galaxxis.Application;
using Galaxxis.Engine;
using Galaxxis.Persistence;
using Galaxxis.Tools.LoadTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Reports and progress lines use the same number format on every machine (1.5, not 1,5).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    // The command line holds commands ("run --players 1000"), not configuration values, so it is not passed to the
    // configuration system. Settings come from the appsettings files and environment variables.
    Args = [],

    // Read the appsettings files from the folder of the executable, regardless of the directory
    // the tool is started from.
    ContentRootPath = AppContext.BaseDirectory,
});

// Machine-specific secrets (connection strings) live in appsettings.Local.json, which is git-ignored and copied next
// to the executable by the build. Environment variables are added again afterwards so they keep the highest priority
// (later sources win); that is how a load test is pointed at its own databases.
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services
    .AddGalaxxisPersistence(builder.Configuration)
    .AddGalaxxisApplication(builder.Configuration)
    .AddGalaxxisEngine(builder.Configuration);

using IHost host = builder.Build();
ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

if (args.Length == 0 || args[0] == "help")
{
    Console.WriteLine(LoadTestCommands.Usage);
    return args.Length == 0 ? 1 : 0;
}

try
{
    // Validate all settings registered with ValidateOnStart before the host starts, so a configuration mistake is
    // reported as a short, readable message instead of a stack trace. Starting the host applies migrations if
    // Persistence:ApplyMigrationsOnStartup is enabled and starts the engine.
    host.Services.GetRequiredService<IStartupValidator>().Validate();
    await host.StartAsync();
}
catch (OptionsValidationException exception)
{
    foreach (string failure in exception.Failures)
    {
        logger.LogError("Invalid configuration: {Failure}", failure);
    }

    return 1;
}

int exitCode = await LoadTestCommands.RunAsync(host.Services, args);

await host.StopAsync();
return exitCode;
