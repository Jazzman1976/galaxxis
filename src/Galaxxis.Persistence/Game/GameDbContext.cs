using Galaxxis.Persistence.Game.Configurations;
using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Persistence.Game;

/// <summary>
/// Database context of the game world (database <c>GalaxxisGame</c>): universes, maps, players, their
/// planets, ships and defenses, and all game events.
/// </summary>
/// <remarks>
/// <para>
/// <b>Code-First.</b> The C# entities in <c>Game/Entities</c> and their configurations in
/// <c>Game/Configurations</c> define the schema. Changes are recorded as migrations in
/// <c>Game/Migrations</c> (see README.md).
/// </para>
/// <para>
/// <b>How to get one.</b> Web requests and application services receive it from dependency injection
/// (scoped: one context per request). The engine uses <c>IDbContextFactory&lt;GameDbContext&gt;</c> and
/// creates a short-lived context per event, so one event can never leave half-changed entities behind for
/// the next.
/// </para>
/// </remarks>
/// <param name="options">Connection and provider settings, supplied by dependency injection.</param>
public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    /// <summary>All universes.</summary>
    public DbSet<Universe> Universes => Set<Universe>();

    /// <summary>All galaxies of all universes.</summary>
    public DbSet<Galaxy> Galaxies => Set<Galaxy>();

    /// <summary>All star systems of all universes.</summary>
    public DbSet<StarSystem> StarSystems => Set<StarSystem>();

    /// <summary>All planets of all universes.</summary>
    public DbSet<Planet> Planets => Set<Planet>();

    /// <summary>All moons of all universes.</summary>
    public DbSet<Moon> Moons => Set<Moon>();

    /// <summary>All players of all universes.</summary>
    public DbSet<Player> Players => Set<Player>();

    /// <summary>All ships.</summary>
    public DbSet<Ship> Ships => Set<Ship>();

    /// <summary>All defenses.</summary>
    public DbSet<Defense> Defenses => Set<Defense>();

    /// <summary>All game events, pending and finished.</summary>
    public DbSet<GameEvent> GameEvents => Set<GameEvent>();

    /// <summary>Applies the entity configurations. Called by EF Core when the model is built.</summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UniverseConfiguration());
        modelBuilder.ApplyConfiguration(new GalaxyConfiguration());
        modelBuilder.ApplyConfiguration(new StarSystemConfiguration());
        modelBuilder.ApplyConfiguration(new PlanetConfiguration());
        modelBuilder.ApplyConfiguration(new MoonConfiguration());
        modelBuilder.ApplyConfiguration(new PlayerConfiguration());
        modelBuilder.ApplyConfiguration(new ShipConfiguration());
        modelBuilder.ApplyConfiguration(new DefenseConfiguration());
        modelBuilder.ApplyConfiguration(new GameEventConfiguration());
    }
}
