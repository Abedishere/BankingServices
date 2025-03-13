using BankingServices.Models;
using BankingServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BankingServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionLogsController : ControllerBase
    {
        private readonly ITransactionLogService _transactionLogService;
        private readonly ILoggingService _loggingService;

        public TransactionLogsController(
            ITransactionLogService transactionLogService,
            ILoggingService loggingService)
        {
            _transactionLogService = transactionLogService;
            _loggingService = loggingService;
        }

        // POST /transaction-logs
        [HttpPost]
        public async Task<IActionResult> CreateTransactionLog(TransactionLog transactionLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdLog = await _transactionLogService.CreateTransactionLogAsync(transactionLog);
                return CreatedAtAction(
                    nameof(GetTransactionLogsByAccountId),
                    new { accountId = createdLog.AccountId },
                    new
                    {
                        TransactionLogId = createdLog.Id,
                        createdLog.AccountId,
                        createdLog.TransactionType,
                        createdLog.Amount,
                        createdLog.Timestamp,
                        createdLog.Status
                    });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating transaction log");
                return StatusCode(500, "An error occurred while creating the transaction log");
            }
        }

        // GET /transaction-logs/{accountId}
        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetTransactionLogsByAccountId(long accountId)
        {
            try
            {
                var logs = await _transactionLogService.GetTransactionLogsByAccountIdAsync(accountId);
                var results = logs.Select(log => new
                {
                    TransactionLogId = log.Id,
                    log.TransactionType,
                    log.Amount,
                    log.Timestamp,
                    log.Status
                });

                return Ok(results);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving transaction logs for account {AccountId}", accountId);
                return StatusCode(500, "An error occurred while retrieving transaction logs");
            }
        }

        // GET /transaction-logs (OData)
        [HttpGet]
        [EnableQuery]
        public IActionResult GetTransactionLogs()
        {
            try
            {
                return Ok(_transactionLogService.GetTransactionLogsQueryable());
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving transaction logs");
                return StatusCode(500, "An error occurred while retrieving transaction logs");
            }
        }
    }
}