using BankingServices.Models.DTOs;
using BankingServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILoggingService _loggingService;

        public AccountsController(
            IAccountService accountService,
            ILoggingService loggingService)
        {
            _accountService = accountService;
            _loggingService = loggingService;
        }

        // GET /accounts/common-transactions?accountIds=1&accountIds=2&accountIds=3
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

        // GET /accounts/balance-summary/{userId}
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
    }
}
