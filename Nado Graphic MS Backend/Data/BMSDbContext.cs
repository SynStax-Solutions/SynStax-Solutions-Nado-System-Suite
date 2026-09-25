using Microsoft.EntityFrameworkCore;
using Nado_Graphics_MS_Backend.Models;
namespace Nado_Graphics_MS_Backend.Data
{
    public class BMSDbContext : DbContext
    {
        public BMSDbContext(DbContextOptions<BMSDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Organisation> Organisations { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<JobCard> JobCards { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Organisation>().ToTable("Organisation");
            modelBuilder.Entity<Client>().ToTable("Client");
            modelBuilder.Entity<Supplier>().ToTable("Supplier");
            modelBuilder.Entity<JobCard>().ToTable("JobCard");
        }
    }
}