using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxxis.Persistence.Game.Configurations;

/// <summary>Table <c>Players</c>.</summary>
internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players");

        // 450 characters is the length ASP.NET Core Identity uses for user ids.
        builder.Property(player => player.AccountId).HasMaxLength(450);
        builder.Property(player => player.Race).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(player => player.Universe)
            .WithMany()
            .HasForeignKey(player => player.UniverseId)
            .OnDelete(DeleteBehavior.NoAction);

        // One player per login account per universe.
        builder.HasIndex(player => new { player.UniverseId, player.AccountId }).IsUnique();
    }
}

/// <summary>Table <c>Ships</c>.</summary>
internal sealed class ShipConfiguration : IEntityTypeConfiguration<Ship>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Ship> builder)
    {
        builder.ToTable("Ships");
        builder.Property(ship => ship.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(ship => ship.Strategy).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(ship => ship.Player).WithMany().HasForeignKey(ship => ship.PlayerId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(ship => ship.Planet).WithMany().HasForeignKey(ship => ship.PlanetId).OnDelete(DeleteBehavior.NoAction);
    }
}

/// <summary>Table <c>Defenses</c>.</summary>
internal sealed class DefenseConfiguration : IEntityTypeConfiguration<Defense>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Defense> builder)
    {
        builder.ToTable("Defenses");
        builder.Property(defense => defense.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(defense => defense.Strategy).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(defense => defense.Player).WithMany().HasForeignKey(defense => defense.PlayerId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(defense => defense.Planet).WithMany().HasForeignKey(defense => defense.PlanetId).OnDelete(DeleteBehavior.NoAction);
    }
}
