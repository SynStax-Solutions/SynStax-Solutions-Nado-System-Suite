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
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<Enquiry> Enquiries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Organisation>().ToTable("Organisation");
            modelBuilder.Entity<Client>().ToTable("Client");
            modelBuilder.Entity<Supplier>().ToTable("Supplier");
            modelBuilder.Entity<JobCard>().ToTable("JobCard");
            
            
            
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<Product>()
                .HasOne<ProductCategory>()
                .WithMany()
                .HasForeignKey(p => p.ProductCategoryID)
                .OnDelete(DeleteBehavior.Restrict);
            
           
            modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory");

            modelBuilder.Entity<Enquiry>().ToTable("Enquiry");

            modelBuilder.Entity<Quote>().ToTable("Quote");
            modelBuilder.Entity<Quote>()
                .HasOne<Client>()
                .WithMany()
                .HasForeignKey(q => q.ClientID);
            modelBuilder.Entity<Quote>()
                .HasIndex(q => q.ReferenceNumber)
                .IsUnique();
            modelBuilder.Entity<Quote>()
                .HasMany(q => q.QuoteItems)
                .WithOne()
                .HasForeignKey(qi => qi.QuoteID)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<QuoteItem>().ToTable("QuoteItem");
            modelBuilder.Entity<QuoteItem>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(qi => qi.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>().ToTable("Invoice");
            modelBuilder.Entity<Invoice>()
                .HasOne<Client>()
                .WithMany()
                .HasForeignKey(i => i.ClientID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Invoice>()
                .HasOne<Quote>()
                .WithMany()
                .HasForeignKey(i => i.QuoteID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Invoice>()
                .HasIndex(q => q.ReferenceNumber)
                .IsUnique();
            modelBuilder.Entity<Invoice>()
                .HasMany(i => i.InvoiceItems)
                .WithOne()
                .HasForeignKey(ii => ii.InvoiceID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>().ToTable("InvoiceItem");
            modelBuilder.Entity<InvoiceItem>()
                .HasOne<Product>()
                .WithMany()
                .HasForeignKey(qi => qi.ProductID)
                .OnDelete(DeleteBehavior.Restrict);
            
            
            modelBuilder.Entity<Payment>().ToTable("Payment");
            modelBuilder.Entity<Payment>()
                .HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(p => p.InvoiceID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>()
                .HasOne<Client>()
                .WithMany()
                .HasForeignKey(p => p.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentMethod)
            .HasConversion<string>();

        }
    }
}
//---------------------------------------------------------------END OF FILE---------------------------------------------------