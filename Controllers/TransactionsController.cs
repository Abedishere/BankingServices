using BankingServices.Data;
using BankingServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankingServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(BankingDbContext context, ILogger<TransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Sends transaction notifications in the preferred language (Accept-Language header).
        /// Supported languages: en, es, fr.
        /// </summary>
        /// <param name="request">Transaction notify request containing TransactionId</param>
        /// <returns>A notification message in the requested language</returns>
        // POST /transactions/notify
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyTransaction([FromBody] TransactionNotifyRequest request, [FromHeader(Name = "Accept-Language")] string? language = "en")
        {
            try
            {
                var transaction = await _context.TransactionLogs.FirstOrDefaultAsync(t => t.Id == request.TransactionId);
                if (transaction == null)
                {
                    return NotFound($"Transaction with ID {request.TransactionId} not found.");
                }

                // Select the correct description field based on the language header
                var localizedDescription = language switch
                {
                    "es" => transaction.DescriptionEs,
                    "fr" => transaction.DescriptionFr,
                    _ => transaction.DescriptionEn
                };

                // Here you'd integrate with an actual notification service (e.g., email, SMS).
                // For demonstration, we'll return a message.
                var notificationMessage = $"Notification for Transaction {transaction.Id}: {localizedDescription}";

                return Ok(notificationMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for transaction {TransactionId}", request.TransactionId);
                return StatusCode(500, "An error occurred while sending the transaction notification.");
            }
        }
    }

    public class TransactionNotifyRequest
    {
        public long TransactionId { get; set; }
    }
}
