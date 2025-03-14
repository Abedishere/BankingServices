// Data/BankingDbContext.cs
using BankingServices.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options)
        {
        }

        public DbSet<TransactionLog> TransactionLogs { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Log> Logs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure TransactionLog entity
            modelBuilder.Entity<TransactionLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .UseIdentityByDefaultColumn()
                    .HasColumnName("id");

                entity.Property(e => e.TransactionType)
                    .IsRequired()
                    .HasColumnName("transaction_type");

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("amount");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnName("status");

                entity.Property(e => e.Timestamp)
                    .HasColumnName("timestamp");

                entity.Property(e => e.Details)
                    .HasColumnName("details");

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id");

                // PostgreSQL-specific table name
                entity.ToTable("transaction_logs");
            });

            // Configure Account entity
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .UseIdentityByDefaultColumn()
                    .HasColumnName("id");

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");

                entity.Property(e => e.AccountType)
                    .IsRequired()
                    .HasColumnName("account_type");

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasColumnName("account_number");

                entity.Property(e => e.CurrentBalance)
                    .HasColumnType("decimal(18,2)")
                    .HasColumnName("current_balance");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                // Define relationship with TransactionLog
                entity.HasMany(a => a.Transactions)
                    .WithOne()
                    .HasForeignKey(t => t.AccountId)
                    .HasPrincipalKey(a => a.Id)
                    .OnDelete(DeleteBehavior.Cascade);

                // PostgreSQL-specific table name
                entity.ToTable("accounts");
            });
            
            // Configure Log entity
            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .UseIdentityByDefaultColumn()
                    .HasColumnName("id");
                    
                entity.Property(e => e.RequestId)
                    .IsRequired()
                    .HasColumnName("request_id");
                    
                entity.Property(e => e.RequestObject)
                    .IsRequired()
                    .HasColumnName("request_object")
                    .HasColumnType("jsonb");
                    
                entity.Property(e => e.RouteURL)
                    .IsRequired()
                    .HasColumnName("route_url");
                    
                entity.Property(e => e.Timestamp)
                    .IsRequired()
                    .HasColumnName("timestamp");
                    
                // Create indexes for performance optimization
                entity.HasIndex(e => e.RequestId)
                    .HasDatabaseName("idx_logs_request_id");
                    
                entity.HasIndex(e => e.RouteURL)
                    .HasDatabaseName("idx_logs_route_url");
                    
                entity.HasIndex(e => e.Timestamp)
                    .HasDatabaseName("idx_logs_timestamp");
                    
                // PostgreSQL-specific table name
                entity.ToTable("logs");
            });
        }
    }
}