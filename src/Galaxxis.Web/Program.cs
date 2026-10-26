// Galaxxis web application: entry point.
//
// Composes the application from the Galaxxis modules (Persistence, Application, Engine), adds sign-in with
// ASP.NET Core Identity and serves the game pages. The engine runs inside this process as a background service
// (it can be switched off per host with Engine:Enabled = false). No game logic lives here: controllers call the
// services of the Application module, which record commands for the engine.

using System.Globalization;
using Galaxxis.Application;
using Galaxxis.Engine;
using Galaxxis.Persistence;
using Galaxxis.Persistence.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------------------------------------
// Configuration
// ---------------------------------------------------------------------------------------------------------------------

// Machine-specific secrets (connection strings) live in appsettings.Local.json, which is git-ignored and copied next
// to the executable by the build. Environment variables and the command line are added again afterwards so they keep
// the highest priority (later sources win). On Azure, the same keys are set as application settings instead.
builder.Configuration
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Local.json"), optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// ---------------------------------------------------------------------------------------------------------------------
// Services
// ---------------------------------------------------------------------------------------------------------------------

builder.Services
    .AddGalaxxisPersistence(builder.Configuration)
    .AddGalaxxisApplication(builder.Configuration)
    .AddGalaxxisEngine(builder.Configuration);

// Sign-in with cookies, on top of the account rules shared with the tools (unique e-mail, lockout after failed attempts).
builder.Services
    .AddIdentity<AppUser, AppRole>(AccountsModule.ConfigureIdentity)
    .AddGalaxxisAccountStores();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/accessdenied";
});

// Secure by default: every page requires a signed-in account unless it is marked [AllowAnonymous]
// (start page, login, registration, error pages).
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddControllersWithViews(options =>
{
    // Every POST request must carry the antiforgery token that forms and the command script send along. This protects
    // all state-changing requests against cross-site request forgery without an attribute on every action.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// The game is German: numbers and dates are formatted accordingly.
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    CultureInfo german = CultureInfo.GetCultureInfo("de-DE");
    options.DefaultRequestCulture = new RequestCulture(german);
    options.SupportedCultures = [german];
    options.SupportedUICultures = [german];
});

WebApplication app = builder.Build();

// Report configuration mistakes (for example a missing connection string) as short readable messages instead of a
// stack trace. All settings registered with ValidateOnStart are checked here.
try
{
    app.Services.GetRequiredService<IStartupValidator>().Validate();
}
catch (OptionsValidationException exception)
{
    foreach (string failure in exception.Failures)
    {
        app.Logger.LogError("Invalid configuration: {Failure}", failure);
    }

    return 1;
}

// ---------------------------------------------------------------------------------------------------------------------
// Request pipeline
// ---------------------------------------------------------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    // In development the detailed exception page is shown instead.
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Friendly pages for 404, 403 and other status codes that come without a body.
app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Liveness and engine state for monitoring (Azure App Service health check, load balancers).
// Only the overall status (Healthy, Degraded, Unhealthy) is written, no details.
app.MapHealthChecks("/health").AllowAnonymous();

// Conventional routes: /{feature}/{action}/{id}, for example /planet/index/42. The default page is the start page.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Start}/{action=Index}/{id?}");

await app.RunAsync();
return 0;
