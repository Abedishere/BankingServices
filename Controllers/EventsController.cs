using BankingServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingServices.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ILoggingService _loggingService;

        public EventsController(IEventService eventService, ILoggingService loggingService)
        {
            _eventService = eventService;
            _loggingService = loggingService;
        }
        
        // POST /events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdEvent = await _eventService.CreateEventAsync(
                    request.TransactionId,
                    request.EventType,
                    request.Details,
                    request.Timestamp);

                return Ok(new
                {
                    message = "Event created and dispatched successfully",
                    eventId = createdEvent.Id
                });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating domain event");
                return StatusCode(500, "An error occurred while creating the event");
            }
        }
        // GET /events/{transactionId}
        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetEventsByTransactionId(long transactionId)
        {
            try
            {
                var events = await _eventService.GetEventsByTransactionIdAsync(transactionId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving events for transaction {TransactionId}", transactionId);
                return StatusCode(500, "An error occurred while retrieving events");
            }
        }
    }
    
    
}
