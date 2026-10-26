using System.Net;
using System.Text.Json;
using Galaxxis.Application.Universes;
using Galaxxis.Persistence.Configuration;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Galaxxis.Web.Tests;

public class WebApplicationTests(WebTestServer server) : IClassFixture<WebTestServer>
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public void The_tests_run_against_their_own_databases()
    {
        // Guards the test setup itself: never touch a developer's real databases.
        DatabaseConnectionOptions connections = server.Application.Services.GetRequiredService<IOptions<DatabaseConnectionOptions>>().Value;

        Assert.Equal(server.Databases.GameConnectionString, connections.Game);
        Assert.Equal(server.Databases.AccountsConnectionString, connections.Accounts);
    }

    [Fact]
    public async Task Visitors_see_the_start_page_and_are_sent_to_the_login_for_game_pages()
    {
        using Browser visitor = server.OpenBrowser();

        Assert.Contains("Jetzt Galaxxis beitreten", await visitor.GetPageAsync("/"));
        Assert.Equal("Healthy", await visitor.GetPageAsync("/health"));

        HttpResponseMessage planet = await visitor.GetAsync("/planet/index/1");
        Assert.Equal(HttpStatusCode.Redirect, planet.StatusCode);
        Assert.Contains("/account/login", planet.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task A_new_player_registers_enters_a_universe_and_sees_the_home_planet()
    {
        UniverseSummary universe = await server.CreateRunningUniverseAsync();
        using Browser player = server.OpenBrowser();
        await player.RegisterAsync();

        string universes = await player.GetPageAsync("/");
        Assert.Contains(universe.Name, universes);
        Assert.Contains($"/Universe/Join/{universe.Id}", universes);

        // Before joining, the universe map leads to the race selection.
        HttpResponseMessage map = await player.GetAsync($"/universe/index/{universe.Id}");
        Assert.Equal(HttpStatusCode.Redirect, map.StatusCode);

        int planetId = await player.JoinAsync(universe.Id);

        string planetPage = await player.GetPageAsync($"/planet/index/{planetId}");
        Assert.Contains("Planetarisches Kontrollzentrum", planetPage);
        Assert.Contains("Erzmine", planetPage);
        Assert.Contains($"{universe.Name} Universum", await player.GetPageAsync($"/universe/index/{universe.Id}"));
        Assert.Contains("Stufe 1 ausbauen", await player.GetPageAsync($"/building/index/{planetId}?type=OreMine"));
    }

    [Fact]
    public async Task A_build_command_is_recorded_for_the_engine_and_its_status_can_be_followed()
    {
        UniverseSummary universe = await server.CreateRunningUniverseAsync();
        using Browser player = server.OpenBrowser();
        await player.RegisterAsync();
        int planetId = await player.JoinAsync(universe.Id);
        string buildingPage = $"/building/index/{planetId}?type=OreMine";

        JsonElement recorded = await Browser.JsonAsync(await player.PostFormAsync($"/building/build/{planetId}?type=OreMine", buildingPage, asScript: true));

        Assert.Equal("Recorded", recorded.GetProperty("outcome").GetString());
        JsonElement status = await Browser.JsonAsync(await player.GetAsync(recorded.GetProperty("statusUrl").GetString()!));
        Assert.Equal("Requested", status.GetProperty("state").GetString());
        Assert.False(status.GetProperty("isDecided").GetBoolean());

        await using (GameDbContext database = server.Databases.CreateGameContext())
        {
            GameEvent command = await database.GameEvents.SingleAsync(e => e.PlanetId == planetId, Cancellation);
            Assert.Equal(GameEventState.Requested, command.State);
        }

        // A second construction while the first is pending is refused immediately, with a reason.
        JsonElement refused = await Browser.JsonAsync(await player.PostFormAsync($"/building/build/{planetId}?type=Factory", buildingPage, asScript: true));
        Assert.Equal("Refused", refused.GetProperty("outcome").GetString());
        Assert.False(string.IsNullOrEmpty(refused.GetProperty("message").GetString()));

        // Without JavaScript, the form post leads back to the building page.
        HttpResponseMessage plainPost = await player.PostFormAsync($"/building/changespeed/{planetId}?type=OreMine", buildingPage,
            new Dictionary<string, string> { ["speedPercent"] = "50" });
        Assert.Equal(HttpStatusCode.Redirect, plainPost.StatusCode);
        Assert.Contains($"/Building/Index/{planetId}", plainPost.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Planets_and_commands_of_other_players_are_not_found()
    {
        UniverseSummary universe = await server.CreateRunningUniverseAsync();
        using Browser owner = server.OpenBrowser();
        using Browser intruder = server.OpenBrowser();
        await owner.RegisterAsync();
        await intruder.RegisterAsync();
        int ownerPlanet = await owner.JoinAsync(universe.Id);
        int intruderPlanet = await intruder.JoinAsync(universe.Id);

        JsonElement ownCommand = await Browser.JsonAsync(await owner.PostFormAsync(
            $"/building/build/{ownerPlanet}?type=OreMine", $"/planet/index/{ownerPlanet}", asScript: true));

        Assert.Equal(HttpStatusCode.NotFound, (await intruder.GetAsync($"/planet/index/{ownerPlanet}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await intruder.GetAsync($"/building/index/{ownerPlanet}?type=OreMine")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await intruder.GetAsync(ownCommand.GetProperty("statusUrl").GetString()!)).StatusCode);

        HttpResponseMessage foreignBuild = await intruder.PostFormAsync(
            $"/building/build/{ownerPlanet}?type=OreMine", $"/planet/index/{intruderPlanet}", asScript: true);
        Assert.Equal(HttpStatusCode.NotFound, foreignBuild.StatusCode);

        // Order buildings have no page of this kind.
        Assert.Equal(HttpStatusCode.NotFound, (await owner.GetAsync($"/building/index/{ownerPlanet}?type=Shipyard")).StatusCode);

        await using GameDbContext database = server.Databases.CreateGameContext();
        Assert.Equal(1, await database.GameEvents.CountAsync(e => e.PlanetId == ownerPlanet, Cancellation));
    }

    [Fact]
    public async Task State_changing_requests_without_antiforgery_token_are_refused()
    {
        UniverseSummary universe = await server.CreateRunningUniverseAsync();
        using Browser player = server.OpenBrowser();
        await player.RegisterAsync();
        int planetId = await player.JoinAsync(universe.Id);

        HttpResponseMessage forged = await player.PostAsync($"/building/build/{planetId}?type=OreMine", new Dictionary<string, string>(), asScript: true);

        Assert.Equal(HttpStatusCode.BadRequest, forged.StatusCode);
        await using GameDbContext database = server.Databases.CreateGameContext();
        Assert.False(await database.GameEvents.AnyAsync(e => e.PlanetId == planetId, Cancellation));
    }

    [Fact]
    public async Task Only_admins_reach_the_administration_and_can_pause_a_universe()
    {
        UniverseSummary universe = await server.CreateRunningUniverseAsync();

        using Browser player = server.OpenBrowser();
        await player.RegisterAsync();
        HttpResponseMessage denied = await player.GetAsync("/admin");
        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("/account/accessdenied", denied.Headers.Location!.OriginalString);
        Assert.Equal(HttpStatusCode.Redirect, (await player.PostFormAsync($"/admin/pause/{universe.Id}", "/")).StatusCode);
        Assert.False(await IsPausedAsync(universe.Id));

        string adminName = "admin" + Guid.NewGuid().ToString("N")[..8];
        await server.CreateAdminAsync(adminName, Browser.Password);
        using Browser admin = server.OpenBrowser();
        Assert.Equal(HttpStatusCode.Redirect, (await admin.LoginAsync(adminName, Browser.Password)).StatusCode);

        Assert.Contains(universe.Name, await admin.GetPageAsync("/admin"));
        Assert.Equal(HttpStatusCode.Redirect, (await admin.PostFormAsync($"/admin/pause/{universe.Id}", "/admin")).StatusCode);
        Assert.True(await IsPausedAsync(universe.Id));
    }

    [Fact]
    public async Task Admins_generate_and_start_a_universe_on_the_admin_page()
    {
        string adminName = "admin" + Guid.NewGuid().ToString("N")[..8];
        await server.CreateAdminAsync(adminName, Browser.Password);
        using Browser admin = server.OpenBrowser();
        await admin.LoginAsync(adminName, Browser.Password);

        string universeName = "Web" + Guid.NewGuid().ToString("N")[..8];
        Dictionary<string, string> form = new()
        {
            ["Generate.Name"] = universeName,
            ["Generate.Seed"] = "42",
            ["Generate.GalaxyCount"] = "1",
            ["Generate.StarSystemsPerGalaxy"] = "2",
            ["Generate.PlanetsPerStarSystem"] = "3",
            ["Generate.TickLength"] = "00:00:00.500",
            ["Generate.StartImmediately"] = "true",
        };

        // Invalid values are shown on the page and generate nothing.
        HttpResponseMessage refused = await admin.PostFormAsync("/admin/generate", "/admin",
            new Dictionary<string, string>(form) { ["Generate.TickLength"] = "eine Sekunde" });
        Assert.Equal(HttpStatusCode.OK, refused.StatusCode);
        Assert.Contains("Ticklänge", await refused.Content.ReadAsStringAsync(Cancellation));

        HttpResponseMessage created = await admin.PostFormAsync("/admin/generate", "/admin", form);
        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        Assert.Contains(universeName, await admin.GetPageAsync("/admin"));

        await using AsyncServiceScope scope = server.Application.Services.CreateAsyncScope();
        IReadOnlyList<UniverseSummary> universes = await scope.ServiceProvider.GetRequiredService<IUniverseAdminService>().ListAsync(Cancellation);
        UniverseSummary universe = Assert.Single(universes, u => u.Name == universeName);
        Assert.Equal(6, universe.PlanetCount);
        Assert.Equal(42, universe.Seed);
        Assert.Equal(TimeSpan.FromMilliseconds(500), universe.TickLength);
        Assert.False(universe.IsPaused);
    }

    [Fact]
    public async Task Players_cannot_generate_universes()
    {
        using Browser player = server.OpenBrowser();
        await player.RegisterAsync();

        HttpResponseMessage response = await player.PostFormAsync("/admin/generate", "/", new Dictionary<string, string>
        {
            ["Generate.GalaxyCount"] = "1",
            ["Generate.StarSystemsPerGalaxy"] = "1",
            ["Generate.PlanetsPerStarSystem"] = "1",
            ["Generate.TickLength"] = "00:00:01",
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/account/accessdenied", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task Repeated_wrong_passwords_lock_the_account()
    {
        using Browser owner = server.OpenBrowser();
        string userName = await owner.RegisterAsync();

        using Browser attacker = server.OpenBrowser();
        for (int attempt = 0; attempt < 5; attempt++)
        {
            HttpResponseMessage wrong = await attacker.LoginAsync(userName, "Wrong-Password-1!");
            Assert.Equal(HttpStatusCode.OK, wrong.StatusCode);
        }

        HttpResponseMessage correct = await attacker.LoginAsync(userName, Browser.Password);

        Assert.Equal(HttpStatusCode.OK, correct.StatusCode);
        Assert.Contains("gesperrt", await correct.Content.ReadAsStringAsync(Cancellation));
    }

    [Fact]
    public async Task Unknown_pages_show_the_error_page()
    {
        using Browser player = server.OpenBrowser();
        await player.RegisterAsync();

        HttpResponseMessage response = await player.GetAsync("/planet/index/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Nicht gefunden", await response.Content.ReadAsStringAsync(Cancellation));
    }

    private async Task<bool> IsPausedAsync(int universeId)
    {
        await using AsyncServiceScope scope = server.Application.Services.CreateAsyncScope();
        IReadOnlyList<UniverseSummary> universes = await scope.ServiceProvider.GetRequiredService<IUniverseAdminService>().ListAsync(Cancellation);
        return universes.Single(u => u.Id == universeId).IsPaused;
    }
}
