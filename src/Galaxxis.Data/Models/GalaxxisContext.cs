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
        public virtual DbSet<Moon> Moon { get; set; }
        public virtual DbSet<Planet> Planet { get; set; }
        public virtual DbSet<System> System { get; set; }
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
                entity.Property(e => e.PlanetFields).HasDefaultValueSql("((150))");

                entity.Property(e => e.PlanetName).HasMaxLength(50);

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
