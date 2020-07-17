using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public virtual DbSet<Galaxy> Galaxy { get; set; }
        public virtual DbSet<GalaxyToUniverse> GalaxyToUniverse { get; set; }
        public virtual DbSet<Moon> Moon { get; set; }
        public virtual DbSet<MoonToPlanet> MoonToPlanet { get; set; }
        public virtual DbSet<Planet> Planet { get; set; }
        public virtual DbSet<PlanetToSystem> PlanetToSystem { get; set; }
        public virtual DbSet<System> System { get; set; }
        public virtual DbSet<SystemToGalaxy> SystemToGalaxy { get; set; }
        public virtual DbSet<Universe> Universe { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=HAWK\\SQLEXPRESS;Database=GalaxxisPrivat;User Id=GalaxxisPrivatUser;Password=***REMOVED***;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Galaxy>(entity =>
            {
                entity.Property(e => e.GalaxyName)
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<GalaxyToUniverse>(entity =>
            {
                entity.HasKey(e => new { e.GalaxyId, e.UniverseId });

                entity.HasOne(d => d.Galaxy)
                    .WithMany(p => p.GalaxyToUniverse)
                    .HasForeignKey(d => d.GalaxyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GalaxyToUniverse_Galaxy");

                entity.HasOne(d => d.Universe)
                    .WithMany(p => p.GalaxyToUniverse)
                    .HasForeignKey(d => d.UniverseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GalaxyToUniverse_Universe");
            });

            modelBuilder.Entity<Moon>(entity =>
            {
                entity.Property(e => e.MoonName).HasMaxLength(50);
            });

            modelBuilder.Entity<MoonToPlanet>(entity =>
            {
                entity.HasKey(e => new { e.MoonId, e.PlanetId });

                entity.HasOne(d => d.Moon)
                    .WithMany(p => p.MoonToPlanet)
                    .HasForeignKey(d => d.MoonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MoonToPlanet_Moon");

                entity.HasOne(d => d.Planet)
                    .WithMany(p => p.MoonToPlanet)
                    .HasForeignKey(d => d.PlanetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MoonToPlanet_Planet");
            });

            modelBuilder.Entity<Planet>(entity =>
            {
                entity.Property(e => e.PlanetName).HasMaxLength(50);
            });

            modelBuilder.Entity<PlanetToSystem>(entity =>
            {
                entity.HasKey(e => new { e.PlanetId, e.SystemId });

                entity.HasOne(d => d.Planet)
                    .WithMany(p => p.PlanetToSystem)
                    .HasForeignKey(d => d.PlanetId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlanetToSystem_Planet");

                entity.HasOne(d => d.System)
                    .WithMany(p => p.PlanetToSystem)
                    .HasForeignKey(d => d.SystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PlanetToSystem_System");
            });

            modelBuilder.Entity<System>(entity =>
            {
                entity.Property(e => e.SystemName)
                    .HasMaxLength(10)
                    .IsFixedLength();
            });

            modelBuilder.Entity<SystemToGalaxy>(entity =>
            {
                entity.HasKey(e => new { e.SystemId, e.GalaxyId });

                entity.HasOne(d => d.Galaxy)
                    .WithMany(p => p.SystemToGalaxy)
                    .HasForeignKey(d => d.GalaxyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SystemToGalaxy_Galaxy");

                entity.HasOne(d => d.System)
                    .WithMany(p => p.SystemToGalaxy)
                    .HasForeignKey(d => d.SystemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SystemToGalaxy_System");
            });

            modelBuilder.Entity<Universe>(entity =>
            {
                entity.Property(e => e.UniverseName).HasMaxLength(50);

                entity.Property(e => e.UniverseTimePerTick).HasDefaultValueSql("((10000000))");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserFirstname).HasMaxLength(50);

                entity.Property(e => e.UserLastname).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
