using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Galaxxis.Data.Models
{
    public partial class GalaxxisContext : DbContext
    {
        public GalaxxisContext()
        {
        }

        public GalaxxisContext(DbContextOptions<GalaxxisContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<EventLog> EventLog { get; set; }
        public virtual DbSet<Galaxy> Galaxy { get; set; }
        public virtual DbSet<Moon> Moon { get; set; }
        public virtual DbSet<Planet> Planet { get; set; }
        public virtual DbSet<Ship> Ship { get; set; }
        public virtual DbSet<System> System { get; set; }
        public virtual DbSet<Universe> Universe { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=Wall-E;Database=Galaxxis;User Id=GalaxxisUser;Password=***REMOVED***;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasIndex(e => new { e.EventTick, e.EventLocation, e.EventLocationId }, "ix_event_target");

                entity.Property(e => e.EventData).HasColumnType("xml");

                entity.Property(e => e.EventLocation)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength(true);

                entity.Property(e => e.EventName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength(true);

                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<EventLog>(entity =>
            {
                entity.Property(e => e.EventLogId).ValueGeneratedNever();

                entity.Property(e => e.EventData).HasColumnType("xml");

                entity.Property(e => e.EventLocation)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength(true);

                entity.Property(e => e.EventName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength(true);

                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<Galaxy>(entity =>
            {
                entity.Property(e => e.GalaxyName).HasMaxLength(50);

                entity.HasOne(d => d.GalaxyUniverse)
                    .WithMany(p => p.Galaxy)
                    .HasForeignKey(d => d.GalaxyUniverseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Galaxy_Universe");
            });

            modelBuilder.Entity<Moon>(entity =>
            {
                entity.Property(e => e.MoonFields).HasDefaultValueSql("((20))");

                entity.Property(e => e.MoonName).HasMaxLength(50);

                entity.HasOne(d => d.MoonPlanet)
                    .WithMany(p => p.Moon)
                    .HasForeignKey(d => d.MoonPlanetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Moon_Planet");
            });

            modelBuilder.Entity<Planet>(entity =>
            {
                entity.Property(e => e.PlanetAntimatter).HasDefaultValueSql("((175))");

                entity.Property(e => e.PlanetCrystalMineUsage).HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanetCrystals).HasDefaultValueSql("((175))");

                entity.Property(e => e.PlanetElementium).HasDefaultValueSql("((75))");

                entity.Property(e => e.PlanetElementiumSynthesizerUsage).HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanetEnergy).HasDefaultValueSql("((200))");

                entity.Property(e => e.PlanetFactoryUsage).HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanetFields).HasDefaultValueSql("((150))");

                entity.Property(e => e.PlanetMageAcademyUsage).HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanetMana).HasDefaultValueSql("((175))");

                entity.Property(e => e.PlanetName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PlanetNeutronAcceleratorUsage).HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanetOre).HasDefaultValueSql("((200))");

                entity.Property(e => e.PlanetOreMineUsage).HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanetPowerSupplyUsage).HasDefaultValueSql("((1))");

                entity.Property(e => e.PlanetTools).HasDefaultValueSql("((200))");

                entity.HasOne(d => d.PlanetSystem)
                    .WithMany(p => p.Planet)
                    .HasForeignKey(d => d.PlanetSystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Planet_System");

                entity.HasOne(d => d.PlanetUser)
                    .WithMany(p => p.Planet)
                    .HasForeignKey(d => d.PlanetUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Planet_User");
            });

            modelBuilder.Entity<Ship>(entity =>
            {
                entity.Property(e => e.ShipTactical)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('Offensive')");

                entity.Property(e => e.ShipType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('Explorer')");

                entity.HasOne(d => d.ShipUser)
                    .WithMany(p => p.Ship)
                    .HasForeignKey(d => d.ShipUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Ship_User");
            });

            modelBuilder.Entity<System>(entity =>
            {
                entity.Property(e => e.SystemName).HasMaxLength(50);

                entity.HasOne(d => d.SystemGalaxy)
                    .WithMany(p => p.System)
                    .HasForeignKey(d => d.SystemGalaxyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_System_Galaxy");
            });

            modelBuilder.Entity<Universe>(entity =>
            {
                entity.Property(e => e.UniverseName).HasMaxLength(50);

                entity.Property(e => e.UniverseTimePerTick).HasDefaultValueSql("((10000000))");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserId, "UK_UserEmail")
                    .IsUnique();

                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserFirstname).HasMaxLength(50);

                entity.Property(e => e.UserLastname).HasMaxLength(50);

                entity.Property(e => e.UserRace)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsFixedLength(true);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
