using System;
using System.Linq;  // Needed for .Any()
using BankingServices.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BankingDbContext context)
        {
            // Applies any pending EF Core migrations for PostgreSQL
            context.Database.Migrate();

            
            if (!context.TransactionLogs.Any())
            {
                context.TransactionLogs.Add(new TransactionLog
                {
                    AccountId = 1,
                    TransactionType = "Deposit",
                    Amount = 1000.00m,
                    Timestamp = DateTime.UtcNow,
                    Status = "Completed",
                    Details = "Initial seed transaction"
                });
                context.SaveChanges();
            }
        }
    }
}