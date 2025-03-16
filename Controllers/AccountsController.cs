using BankingServices.Data;
using BankingServices.Models;
using BankingServices.Models.DTO;
using BankingServices.Models.DTOs;
using BankingServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILoggingService _loggingService;
        private readonly BankingDbContext _dbContext;

        public AccountsController(
            IAccountService accountService,
            ILoggingService loggingService,
            BankingDbContext dbContext)
        {
            _accountService = accountService;
            _loggingService = loggingService;
            _dbContext = dbContext;
        }
        
        [HttpGet("common-transactions")]
        public async Task<IActionResult> GetCommonTransactions([FromQuery] List<long> accountIds)
        {
            if (accountIds == null || accountIds.Count < 2)
            {
                return BadRequest("At least two account IDs are required.");
            }

            try
            {
                var result = await _accountService.GetCommonTransactionsAsync(accountIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving common transactions");
                return StatusCode(500, "An error occurred while retrieving common transactions.");
            }
        }
        
        [HttpGet("balance-summary/{userId}")]
        public async Task<IActionResult> GetAccountBalanceSummary(long userId)
        {
            try
            {
                var result = await _accountService.GetAccountBalanceSummaryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving account balance summary for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving account balance summary.");
            }
        }
        
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferFunds([FromBody] TransferFundsRequest request)
        {
            if (request == null || request.FromAccountId <= 0 || request.ToAccountId <= 0 || request.Amount <= 0)
            {
                return BadRequest("Invalid transfer request.");
            }

            try
            {
                bool success = await _accountService.TransferFundsAsync(
                    request.FromAccountId,
                    request.ToAccountId,
                    request.Amount
                );

                if (success)
                    return Ok("Transfer completed successfully.");
                else
                    return StatusCode(500, "Transfer failed.");
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error processing fund transfer");
                return StatusCode(500, "An error occurred during fund transfer.");
            }
        }
        
        [HttpGet("{accountId}/details")]
        public async Task<IActionResult> GetAccountDetails(
            long accountId,
            [FromHeader(Name = "Accept-Language")] string? language = "en")
        {
            try
            {
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
                if (account == null)
                {
                    return NotFound($"Account with ID {accountId} not found.");
                }

                // Select the appropriate localized name based on the language header.
                var localizedName = language?.ToLower() switch
                {
                    "es" => account.NameEs,
                    "fr" => account.NameFr,
                    _ => account.NameEn // Default to English
                };

                var result = new
                {
                    AccountId = account.Id,
                    Name = localizedName,
                    AccountType = account.AccountType,
                    AccountNumber = account.AccountNumber,
                    CurrentBalance = account.CurrentBalance,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving account details for ID: {AccountId}", accountId);
                return StatusCode(500, "An error occurred while retrieving account details.");
            }
        }
    }
    
    public class TransferFundsRequest
    {
        public long FromAccountId { get; set; }
        public long ToAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
