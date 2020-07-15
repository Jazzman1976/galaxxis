using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Created database first. https://www.entityframeworktutorial.net/efcore/create-model-for-existing-database-in-ef-core.aspx
/// </summary>
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
