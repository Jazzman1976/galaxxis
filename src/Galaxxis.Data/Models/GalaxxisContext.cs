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
        public virtual DbSet<Universe> Universe { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=HAWK\\SQLEXPRESS;Database=GalaxxisPrivat;Trusted_Connection=True;");
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
