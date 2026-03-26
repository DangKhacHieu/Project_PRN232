using DAL.Entities;
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
        public DbSet<ChangePasswordOtp> ChangePasswordOtps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Invoice>()
                .HasIndex(i => new { i.ContractId, i.Month, i.Year })
                .IsUnique()
                .HasDatabaseName("UQ_invoice");

            modelBuilder.Entity<UtilityReading>()
                .HasIndex(u => new { u.StallId, u.Month, u.Year })
                .IsUnique()
                .HasDatabaseName("UQ_reading");


            modelBuilder.Entity<Stall>()
                .ToTable(t => t.HasCheckConstraint("CK_Stall_Status", "status IN ('VACANT','RENTED','MAINTENANCE','DISPUTE')"));

            modelBuilder.Entity<StallContract>()
                .ToTable(t => t.HasCheckConstraint("CK_Contract_Status", "status IN ('ACTIVE','EXPIRED','TERMINATED')"));

            modelBuilder.Entity<Invoice>()
                .ToTable(t => {
                    t.HasCheckConstraint("CK_Invoice_Status", "status IN ('UNPAID','PAID','OVERDUE')");
                    t.HasCheckConstraint("CK_Invoice_Month", "month BETWEEN 1 AND 12");
                });

            modelBuilder.Entity<Payment>()
                .ToTable(t => {
                    t.HasCheckConstraint("CK_Payment_Method", "payment_method IN ('QR','CASH','BANK_TRANSFER')");
                    t.HasCheckConstraint("CK_Payment_Status", "status IN ('PENDING','SUCCESS','FAILED','REFUNDED','CANCELLED')");
                });

            modelBuilder.Entity<UtilityReading>()
                .ToTable(t => t.HasCheckConstraint("CK_Utility_Month", "month BETWEEN 1 AND 12"));

            modelBuilder.Entity<SupportTicket>()
                .ToTable(t => t.HasCheckConstraint("CK_Ticket_Status", "status IN ('PENDING','PROCESSING','RESOLVED','CLOSED')"));


            // --- Cấu hình DEFAULT GETDATE() ---
            modelBuilder.Entity<User>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Market>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Zone>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Stall>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<VendorProfile>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<StallContract>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<ContractHistory>().Property(e => e.ActionDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<UtilityReading>().Property(e => e.RecordedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Invoice>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Payment>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Product>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<PriceHistory>().Property(e => e.EffectiveTime).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<SupportTicket>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<ChangePasswordOtp>().Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            // --- Cấu hình DEFAULT TEXT / BOOLEAN ---
            modelBuilder.Entity<User>().Property(e => e.IsActive).HasDefaultValue(true);
            modelBuilder.Entity<Product>().Property(e => e.IsActive).HasDefaultValue(true);

            modelBuilder.Entity<Stall>().Property(e => e.Status).HasDefaultValue("VACANT");
            modelBuilder.Entity<StallContract>().Property(e => e.Status).HasDefaultValue("ACTIVE");
            modelBuilder.Entity<Invoice>().Property(e => e.Status).HasDefaultValue("UNPAID");
            modelBuilder.Entity<Payment>().Property(e => e.Status).HasDefaultValue("PENDING");
            modelBuilder.Entity<SupportTicket>().Property(e => e.Status).HasDefaultValue("PENDING");
        }
    }
}