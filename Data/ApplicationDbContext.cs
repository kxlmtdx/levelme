using Microsoft.EntityFrameworkCore;
using levelme.Models;

namespace levelme.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Accounts> Accounts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Accounts>()
                .HasIndex(a => a.Username)
                .IsUnique();

            modelBuilder.Entity<Accounts>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }
    }
}