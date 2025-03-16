using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankingServices.Data;
using BankingServices.Models;
using BankingServices.Models.DTOs;
using BankingServices.Services;
using BankingServices.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BankingServices.Tests
{
    #region Account Management Tests

    public class AccountManagementTests
    {
        private BankingDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<BankingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new BankingDbContext(options);
        }

        [Fact]
        public async Task CreateAccount_WithValidInput_ShouldSucceed()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<AccountService>>();
            var unitOfWork = new UnitOfWork.UnitOfWork(context);
            var account = new Account
            {
                UserId = 1,
                AccountType = "Savings",
                AccountNumber = "ACC123",
                CurrentBalance = 0,
                NameEn = "My Savings",
                NameEs = "Mis Ahorros",
                NameFr = "Mes Économies"
            };
            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            // Act: retrieve account balance summary
            var accountService = new AccountService(context, loggerMock.Object, unitOfWork);
            var summary = await accountService.GetAccountBalanceSummaryAsync(1);

            // Assert
            Assert.NotNull(summary);
            Assert.Equal(1, summary.TotalAccounts);
            Assert.Equal("ACC123", summary.Accounts.First().AccountNumber);
        }

        [Fact]
        public async Task UpdateAccount_WithValidChanges_ShouldReflectUpdates()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<AccountService>>();
            var unitOfWork = new UnitOfWork.UnitOfWork(context);
            var account = new Account
            {
                UserId = 1,
                AccountType = "Checking",
                AccountNumber = "CHK001",
                CurrentBalance = 100,
                NameEn = "My Checking",
                NameEs = "Mi Chequera",
                NameFr = "Mon Chèque"
            };
            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            // Act: update account's English name
            account.NameEn = "Updated Checking";
            context.Accounts.Update(account);
            await context.SaveChangesAsync();

            var updatedAccount = await context.Accounts.FindAsync(account.Id);

            // Assert
            Assert.Equal("Updated Checking", updatedAccount.NameEn);
        }
    }

    #endregion

    #region Transaction Tests

    public class TransactionTests
    {
        private BankingDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<BankingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new BankingDbContext(options);
        }

        [Fact]
        public async Task TransferFunds_WithValidInput_ShouldUpdateBalances()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<AccountService>>();
            var unitOfWork = new UnitOfWork.UnitOfWork(context);

            var fromAccount = new Account
            {
                UserId = 1,
                AccountType = "Checking",
                AccountNumber = "CHK100",
                CurrentBalance = 500,
                NameEn = "From Account",
                NameEs = "Cuenta Origen",
                NameFr = "Compte Source"
            };
            var toAccount = new Account
            {
                UserId = 2,
                AccountType = "Checking",
                AccountNumber = "CHK200",
                CurrentBalance = 100,
                NameEn = "To Account",
                NameEs = "Cuenta Destino",
                NameFr = "Compte Destination"
            };
            context.Accounts.AddRange(fromAccount, toAccount);
            await context.SaveChangesAsync();

            var accountService = new AccountService(context, loggerMock.Object, unitOfWork);

            // Act: Transfer 200 from fromAccount to toAccount
            var result = await accountService.TransferFundsAsync(fromAccount.Id, toAccount.Id, 200);

            // Assert
            Assert.True(result);
            var updatedFrom = await context.Accounts.FindAsync(fromAccount.Id);
            var updatedTo = await context.Accounts.FindAsync(toAccount.Id);
            Assert.Equal(300, updatedFrom.CurrentBalance);
            Assert.Equal(300, updatedTo.CurrentBalance);
        }

        [Fact]
        public async Task TransferFunds_WithInsufficientFunds_ShouldFail()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<AccountService>>();
            var unitOfWork = new UnitOfWork.UnitOfWork(context);

            var fromAccount = new Account
            {
                UserId = 1,
                AccountType = "Checking",
                AccountNumber = "CHK101",
                CurrentBalance = 50,
                NameEn = "From Account",
                NameEs = "Cuenta Origen",
                NameFr = "Compte Source"
            };
            var toAccount = new Account
            {
                UserId = 2,
                AccountType = "Checking",
                AccountNumber = "CHK201",
                CurrentBalance = 100,
                NameEn = "To Account",
                NameEs = "Cuenta Destino",
                NameFr = "Compte Destination"
            };
            context.Accounts.AddRange(fromAccount, toAccount);
            await context.SaveChangesAsync();

            var accountService = new AccountService(context, loggerMock.Object, unitOfWork);

            // Act: Attempt to transfer more than available balance
            var result = await accountService.TransferFundsAsync(fromAccount.Id, toAccount.Id, 100);

            // Assert: Transfer should fail and balances remain unchanged
            Assert.False(result);
            var updatedFrom = await context.Accounts.FindAsync(fromAccount.Id);
            var updatedTo = await context.Accounts.FindAsync(toAccount.Id);
            Assert.Equal(50, updatedFrom.CurrentBalance);
            Assert.Equal(100, updatedTo.CurrentBalance);
        }
    }

    #endregion

    #region Notification Tests

    // Simulated Notification entity and service for testing purposes
    public class Notification
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }

    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(long userId);
        Task MarkAsReadAsync(int notificationId);
    }

    public class NotificationService : INotificationService
    {
        private readonly List<Notification> _notifications = new List<Notification>();
        public Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.Id = _notifications.Count + 1;
            _notifications.Add(notification);
            return Task.FromResult(notification);
        }

        public Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(long userId)
        {
            var result = _notifications.Where(n => n.UserId == userId);
            return Task.FromResult(result);
        }

        public Task MarkAsReadAsync(int notificationId)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
                notification.IsRead = true;
            return Task.CompletedTask;
        }
    }

    public class NotificationTests
    {
        [Fact]
        public async Task CreateNotification_WithValidData_ShouldSucceed()
        {
            // Arrange
            var service = new NotificationService();
            var notification = new Notification { UserId = 1, Message = "Test Notification", IsRead = false };

            // Act
            var result = await service.CreateNotificationAsync(notification);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.False(result.IsRead);
        }

        [Fact]
        public async Task GetNotifications_ByUserId_ShouldReturnCorrectCount()
        {
            // Arrange
            var service = new NotificationService();
            await service.CreateNotificationAsync(new Notification { UserId = 1, Message = "Notification 1", IsRead = false });
            await service.CreateNotificationAsync(new Notification { UserId = 2, Message = "Notification 2", IsRead = false });
            await service.CreateNotificationAsync(new Notification { UserId = 1, Message = "Notification 3", IsRead = false });

            // Act
            var notifications = await service.GetNotificationsByUserIdAsync(1);

            // Assert
            Assert.Equal(2, notifications.Count());
        }

        [Fact]
        public async Task MarkNotification_AsRead_ShouldUpdateStatus()
        {
            // Arrange
            var service = new NotificationService();
            var notification = await service.CreateNotificationAsync(new Notification { UserId = 1, Message = "Notification", IsRead = false });

            // Act
            await service.MarkAsReadAsync(notification.Id);
            var notifications = await service.GetNotificationsByUserIdAsync(1);
            var updatedNotification = notifications.FirstOrDefault(n => n.Id == notification.Id);

            // Assert
            Assert.True(updatedNotification.IsRead);
        }
    }

    #endregion

    #region Recurrent Transaction Tests

    // Simulated recurrent transaction entity and service for testing purposes
    public class RecurrentTransaction
    {
        public int Id { get; set; }
        public long AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime NextRun { get; set; }
    }

    public interface IRecurrentTransactionService
    {
        Task<RecurrentTransaction> CreateRecurrentTransactionAsync(RecurrentTransaction rt);
        Task ProcessRecurrentTransactionsAsync(DateTime currentDate);
    }

    public class RecurrentTransactionService : IRecurrentTransactionService
    {
        private readonly List<RecurrentTransaction> _recurrentTransactions = new List<RecurrentTransaction>();
        public Task<RecurrentTransaction> CreateRecurrentTransactionAsync(RecurrentTransaction rt)
        {
            rt.Id = _recurrentTransactions.Count + 1;
            _recurrentTransactions.Add(rt);
            return Task.FromResult(rt);
        }

        public Task ProcessRecurrentTransactionsAsync(DateTime currentDate)
        {
            foreach (var rt in _recurrentTransactions)
            {
                if (currentDate >= rt.NextRun)
                {
                    rt.NextRun = rt.NextRun.AddDays(1);
                }
            }
            return Task.CompletedTask;
        }
    }

    public class RecurrentTransactionTests
    {
        [Fact]
        public async Task CreateRecurrentTransaction_WithValidData_ShouldSucceed()
        {
            // Arrange
            var service = new RecurrentTransactionService();
            var rt = new RecurrentTransaction { AccountId = 1, Amount = 100, NextRun = DateTime.UtcNow.AddHours(1) };

            // Act
            var result = await service.CreateRecurrentTransactionAsync(rt);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task ProcessRecurrentTransactions_ShouldUpdateNextRun()
        {
            // Arrange
            var service = new RecurrentTransactionService();
            var initialNextRun = DateTime.UtcNow.AddHours(-1);
            var rt = new RecurrentTransaction { AccountId = 1, Amount = 50, NextRun = initialNextRun };
            await service.CreateRecurrentTransactionAsync(rt);

            // Act
            await service.ProcessRecurrentTransactionsAsync(DateTime.UtcNow);
            // Retrieve updated transaction from the service's internal list
            var updatedRt = (await service.CreateRecurrentTransactionAsync(rt));

            // Assert: NextRun should be updated (greater than initialNextRun)
            Assert.True(updatedRt.NextRun > initialNextRun);
        }
    }

    #endregion

    #region Event Sourcing Tests

    // Simulated domain event and event sourcing service for testing
    public class DomainEvent
    {
        public int Id { get; set; }
        public long AccountId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public interface IEventSourcingService
    {
        Task<DomainEvent> CreateEventAsync(DomainEvent domainEvent);
        Task<IEnumerable<DomainEvent>> GetEventsByAccountIdAsync(long accountId);
        Task<bool> RollbackEventsAsync(long accountId, DateTime upTo);
    }

    public class EventSourcingService : IEventSourcingService
    {
        private readonly List<DomainEvent> _events = new List<DomainEvent>();
        public Task<DomainEvent> CreateEventAsync(DomainEvent domainEvent)
        {
            domainEvent.Id = _events.Count + 1;
            _events.Add(domainEvent);
            return Task.FromResult(domainEvent);
        }

        public Task<IEnumerable<DomainEvent>> GetEventsByAccountIdAsync(long accountId)
        {
            var result = _events.Where(e => e.AccountId == accountId);
            return Task.FromResult(result);
        }

        public Task<bool> RollbackEventsAsync(long accountId, DateTime upTo)
        {
            _events.RemoveAll(e => e.AccountId == accountId && e.Timestamp <= upTo);
            return Task.FromResult(true);
        }
    }

    public class EventSourcingTests
    {
        [Fact]
        public async Task CreateEvent_WithValidData_ShouldSucceed()
        {
            // Arrange
            var service = new EventSourcingService();
            var domainEvent = new DomainEvent { AccountId = 1, EventType = "Deposit", Timestamp = DateTime.UtcNow };

            // Act
            var result = await service.CreateEventAsync(domainEvent);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task RollbackEvents_ShouldRemoveEventsUpToSpecifiedDate()
        {
            // Arrange
            var service = new EventSourcingService();
            var now = DateTime.UtcNow;
            await service.CreateEventAsync(new DomainEvent { AccountId = 1, EventType = "Deposit", Timestamp = now.AddMinutes(-10) });
            await service.CreateEventAsync(new DomainEvent { AccountId = 1, EventType = "Withdrawal", Timestamp = now.AddMinutes(-5) });
            await service.CreateEventAsync(new DomainEvent { AccountId = 1, EventType = "Transfer", Timestamp = now.AddMinutes(1) });

            // Act
            var rollbackResult = await service.RollbackEventsAsync(1, now);
            var remainingEvents = await service.GetEventsByAccountIdAsync(1);

            // Assert
            Assert.True(rollbackResult);
            // Only events after 'now' should remain.
            Assert.Single(remainingEvents);
        }
    }

    #endregion

    #region Edge Case Tests

    public class EdgeCaseTests
    {
        private BankingDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<BankingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new BankingDbContext(options);
        }

        [Fact]
        public async Task TransferFunds_WithNonExistentAccount_ShouldThrowException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<AccountService>>();
            var unitOfWork = new UnitOfWork.UnitOfWork(context);

            // Create one valid account
            var account = new Account
            {
                UserId = 1,
                AccountType = "Checking",
                AccountNumber = "CHK999",
                CurrentBalance = 100,
                NameEn = "Test Account",
                NameEs = "Cuenta de Prueba",
                NameFr = "Compte Test"
            };
            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            var accountService = new AccountService(context, loggerMock.Object, unitOfWork);

            // Act & Assert: Transfer from non-existent account should throw an exception
            await Assert.ThrowsAsync<Exception>(() => accountService.TransferFundsAsync(999, account.Id, 50));
        }

        [Fact]
        public async Task TransferFunds_WithNegativeAmount_ShouldThrowException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<AccountService>>();
            var unitOfWork = new UnitOfWork.UnitOfWork(context);

            // Create an account
            var account = new Account
            {
                UserId = 1,
                AccountType = "Savings",
                AccountNumber = "SAV123",
                CurrentBalance = 500,
                NameEn = "Savings Account",
                NameEs = "Cuenta de Ahorros",
                NameFr = "Compte d'Épargne"
            };
            context.Accounts.Add(account);
            await context.SaveChangesAsync();

            var accountService = new AccountService(context, loggerMock.Object, unitOfWork);

            // Act & Assert: Negative transfer should throw an exception
            await Assert.ThrowsAsync<ArgumentException>(() => accountService.TransferFundsAsync(account.Id, account.Id, -100));
        }
    }

    #endregion
}
