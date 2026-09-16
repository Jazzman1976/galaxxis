using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galaxxis.Persistence.Game.Configurations;

/// <summary>Table <c>GameEvents</c>.</summary>
internal sealed class GameEventConfiguration : IEntityTypeConfiguration<GameEvent>
{
    /// <summary>
    /// Name of the unique index that allows only one scheduled event per planet and queue.
    /// Code that inserts or schedules events can recognise a violation of this rule by this name.
    /// </summary>
    public const string OneScheduledEventPerQueueIndex = "UX_GameEvents_OneScheduledPerPlanetQueue";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GameEvent> builder)
    {
        builder.ToTable("GameEvents");

        builder.Property(gameEvent => gameEvent.Type).HasMaxLength(100);
        builder.Property(gameEvent => gameEvent.State).HasConversion<string>().HasMaxLength(20);
        builder.Property(gameEvent => gameEvent.Queue).HasMaxLength(50);

        // Events are history and are not removed together with players or planets.
        builder.HasOne(gameEvent => gameEvent.Universe).WithMany().HasForeignKey(gameEvent => gameEvent.UniverseId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(gameEvent => gameEvent.Player).WithMany().HasForeignKey(gameEvent => gameEvent.PlayerId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(gameEvent => gameEvent.Planet).WithMany().HasForeignKey(gameEvent => gameEvent.PlanetId).OnDelete(DeleteBehavior.NoAction);

        // The engine's main query: "which events of this universe are due?", in processing order.
        builder.HasIndex(gameEvent => new { gameEvent.UniverseId, gameEvent.State, gameEvent.ProcessAtTick, gameEvent.Id });

        // Safety net against double execution: at most one scheduled event per planet and queue, enforced
        // by the database itself. Even if two engine instances ever raced, the second insert would fail.
        builder.HasIndex(gameEvent => new { gameEvent.PlanetId, gameEvent.Queue })
            .IsUnique()
            .HasFilter("[State] = N'Scheduled' AND [PlanetId] IS NOT NULL AND [Queue] IS NOT NULL")
            .HasDatabaseName(OneScheduledEventPerQueueIndex);

        // A player's event history.
        builder.HasIndex(gameEvent => new { gameEvent.PlayerId, gameEvent.CreatedAt });
    }
}
