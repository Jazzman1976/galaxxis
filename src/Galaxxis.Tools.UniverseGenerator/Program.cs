// Galaxxis universe generator: console entry point.
//
// The tool creates new universes (galaxies, star systems, planets, moons) and bootstraps the admin
// account. It is a thin host: the actual work is done by services of the Application module, so a
// web admin page can later offer the same functionality without duplicating any logic.
//
// Step 1 of the foundation refactoring only establishes the host (configuration, logging, module
// registration, settings validation). The commands generate / list / resume / bootstrap-admin
// follow in step 3.

using Galaxxis.Application;
using Galaxxis.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // Read the appsettings files from the folder of the executable, regardless of the directory
    // the tool is started from.
    ContentRootPath = AppContext.BaseDirectory,
});

// Machine-specific secrets (connection strings, passwords) live in appsettings.Local.json, which is
// git-ignored and copied next to the executable by the build. Environment variables and command line
// arguments are added again afterwards so they keep the highest priority (later sources win).
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services
    .AddGalaxxisPersistence(builder.Configuration)
    .AddGalaxxisApplication();

using IHost host = builder.Build();
ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    // Validate all settings registered with ValidateOnStart before the host starts, so a
    // configuration mistake is reported as a short, readable message instead of a stack trace.
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

logger.LogInformation(
    "Configuration is valid. Universe generation commands are added in step 3 of the foundation refactoring.");

await host.StopAsync();
return 0;
