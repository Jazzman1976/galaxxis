using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxxis.Persistence.Game.Configurations;

/// <summary>Maximum length of display names (universes, galaxies, star systems, planets, moons).</summary>
internal static class NameColumn
{
    /// <summary>Maximum number of characters.</summary>
    public const int MaxLength = 50;
}

/// <summary>Table <c>Universes</c>.</summary>
internal sealed class UniverseConfiguration : IEntityTypeConfiguration<Universe>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Universe> builder)
    {
        builder.ToTable("Universes");
        builder.Property(universe => universe.Name).HasMaxLength(NameColumn.MaxLength);

        // TimeSpan columns are stored as whole milliseconds in a bigint. SQL Server's "time" type cannot hold
        // more than 24 hours, and a universe can easily be paused for longer in total.
        builder.Property(universe => universe.TickLength)
            .HasConversion(value => (long)value.TotalMilliseconds, value => TimeSpan.FromMilliseconds(value))
            .HasColumnName("TickLengthMilliseconds");
        builder.Property(universe => universe.TotalPausedDuration)
            .HasConversion(value => (long)value.TotalMilliseconds, value => TimeSpan.FromMilliseconds(value))
            .HasColumnName("TotalPausedMilliseconds");

        builder.HasMany(universe => universe.Galaxies)
            .WithOne(galaxy => galaxy.Universe)
            .HasForeignKey(galaxy => galaxy.UniverseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Table <c>Galaxies</c>.</summary>
internal sealed class GalaxyConfiguration : IEntityTypeConfiguration<Galaxy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Galaxy> builder)
    {
        builder.ToTable("Galaxies");
        builder.Property(galaxy => galaxy.Name).HasMaxLength(NameColumn.MaxLength);
        builder.Ignore(galaxy => galaxy.Coordinate);

        builder.HasMany(galaxy => galaxy.StarSystems)
            .WithOne(system => system.Galaxy)
            .HasForeignKey(system => system.GalaxyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Table <c>StarSystems</c>.</summary>
internal sealed class StarSystemConfiguration : IEntityTypeConfiguration<StarSystem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StarSystem> builder)
    {
        builder.ToTable("StarSystems");
        builder.Property(system => system.Name).HasMaxLength(NameColumn.MaxLength);
        builder.Ignore(system => system.Coordinate);

        builder.HasMany(system => system.Planets)
            .WithOne(planet => planet.StarSystem)
            .HasForeignKey(planet => planet.StarSystemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Table <c>Planets</c>.</summary>
internal sealed class PlanetConfiguration : IEntityTypeConfiguration<Planet>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Planet> builder)
    {
        builder.ToTable("Planets");
        builder.Property(planet => planet.Name).HasMaxLength(NameColumn.MaxLength);
        builder.Property(planet => planet.RowVersion).IsRowVersion();

        // Domain views over the columns; not stored themselves.
        builder.Ignore(planet => planet.Coordinate);
        builder.Ignore(planet => planet.Stock);
        builder.Ignore(planet => planet.Buildings);

        // Planets are reached through the map hierarchy (cascade from the star system). The direct link to the
        // universe exists for querying only and must not form a second cascade path, which SQL Server forbids.
        builder.HasOne(planet => planet.Universe)
            .WithMany()
            .HasForeignKey(planet => planet.UniverseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(planet => planet.Owner)
            .WithMany(player => player.Planets)
            .HasForeignKey(planet => planet.OwnerPlayerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(planet => planet.Moons)
            .WithOne(moon => moon.Planet)
            .HasForeignKey(moon => moon.PlanetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Finding planets of a player, and unclaimed planets (owner NULL) in a universe.
        builder.HasIndex(planet => new { planet.UniverseId, planet.OwnerPlayerId });
    }
}

/// <summary>Table <c>Moons</c>.</summary>
internal sealed class MoonConfiguration : IEntityTypeConfiguration<Moon>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Moon> builder)
    {
        builder.ToTable("Moons");
        builder.Property(moon => moon.Name).HasMaxLength(NameColumn.MaxLength);
    }
}
