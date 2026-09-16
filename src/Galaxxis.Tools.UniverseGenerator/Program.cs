// Galaxxis universe generator: console entry point.
//
// Creates new universes (galaxies, star systems, planets, moons), pauses and resumes them, and bootstraps the
// administrator account. The tool is a thin host: all work is done by services of the Application module,
// so a web admin page can later offer the same functionality without duplicating any logic.
//
// Usage: see README.md, or run without arguments for the interactive menu, or with "help".

using Galaxxis.Application;
using Galaxxis.Persistence;
using Galaxxis.Tools.UniverseGenerator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    // The command line holds commands ("generate --seed 42"), not configuration values, so it is not passed
    // to the configuration system. Settings come from the appsettings files and environment variables.
    Args = [],

    // Read the appsettings files from the folder of the executable, regardless of the directory
    // the tool is started from.
    ContentRootPath = AppContext.BaseDirectory,
});

// Machine-specific secrets (connection strings, admin password) live in appsettings.Local.json, which is
// git-ignored and copied next to the executable by the build. Environment variables are added again afterwards
// so they keep the highest priority (later sources win).
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services
    .AddGalaxxisPersistence(builder.Configuration)
    .AddGalaxxisApplication(builder.Configuration);

using IHost host = builder.Build();
ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    // Validate all settings registered with ValidateOnStart before the host starts, so a configuration
    // mistake is reported as a short, readable message instead of a stack trace. Starting the host then
    // applies database migrations if Persistence:ApplyMigrationsOnStartup is enabled.
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

int exitCode = args.Length == 0
    ? await InteractiveMenu.RunAsync(host.Services)
    : await GeneratorCommands.RunAsync(host.Services, args);

await host.StopAsync();
return exitCode;
