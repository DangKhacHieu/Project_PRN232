using DAL.Entities.Finance_Billing;
using DAL.Entities.Infrastructure;
using DAL.Entities.Product_Management;
using DAL.Entities.Support_System;
using DAL.Entities.System_Auth;
using DAL.Entities.Vendor_Contract;
using MarketManagement_DAL.Entities; // Namespace chứa các models của bạn
using MarketManagement_DAL.Entities.Fee_Invoice_Items;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 1. System & Auth
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        // 2. Infrastructure
        public DbSet<Market> Markets { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Stall> Stalls { get; set; }

        // 3. Vendor & Contract
        public DbSet<VendorProfile> VendorProfiles { get; set; }
        public DbSet<StallContract> StallContracts { get; set; }
        public DbSet<ContractHistory> ContractHistories { get; set; }

        // 4. Utility & Finance
        public DbSet<UtilityReading> UtilityReadings { get; set; }
        public DbSet<FeeType> FeeTypes { get; set; }
        public DbSet<FeeConfig> FeeConfigs { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        // 5. Product Management
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }

        // 6. Support System
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<TicketImage> TicketImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc Unique
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.ContractId, i.Month, i.Year })
                .IsUnique();

            modelBuilder.Entity<UtilityReading>()
                .HasIndex(u => new { u.StallId, u.Month, u.Year })
                .IsUnique();
        }
    }
}