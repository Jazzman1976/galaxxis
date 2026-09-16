// Galaxxis load test: console entry point.
//
// The tool simulates many players (target: 1000) submitting commands at the same time and measures
// how the engine copes: processing time per event, backlog of due events, and correctness checks
// such as "no building was upgraded twice" and "no resource stock went negative".
//
// Step 1 of the foundation refactoring only establishes the host (configuration, logging, module
// registration, settings validation). The simulation follows in step 6.

using Galaxxis.Application;
using Galaxxis.Engine;
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
    .AddGalaxxisApplication()
    .AddGalaxxisEngine(builder.Configuration);

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
    "Configuration is valid. The load simulation is added in step 6 of the foundation refactoring.");

await host.StopAsync();
return 0;
