using System;
using System.Linq;
using BankingServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankingServices.Data
{
    public static class DbInitializer
    {
        public static void Initialize(BankingDbContext context)
        {
            // Only use this method if it's called directly
            try
            {
                // Test the connection first
                if (!context.Database.CanConnect())
                {
                    throw new Exception("Cannot connect to the database. Please check your connection string and ensure PostgreSQL is running.");
                }
                
                // Use EnsureCreated instead of Migrate for simplicity
                context.Database.EnsureCreated();
                
                // Seed data if the table is empty
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
            catch (Exception)
            {
                // Simply rethrow the exception to be handled by the caller
                throw;
            }
        }
    }
}