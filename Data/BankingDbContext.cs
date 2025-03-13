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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        }
    }
}