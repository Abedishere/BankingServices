using BankingServices.Data;
using BankingServices.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BankingServices.Services
{
    public class LogService : ILogService
    {
        private readonly BankingDbContext _dbContext;
        private readonly ILogger<LogService> _logger;
        // The following fields are not used in this implementation,
        // so they can be removed or used as needed in future modifications.
        private ILogService _logServiceImplementation;
        private ILogService _logServiceImplementation1;

        public LogService(BankingDbContext dbContext, ILogger<LogService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Log> CreateLogAsync(Log log)
        {
            try
            {
                _dbContext.Logs.Add(log);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Log created successfully: {RequestId}, RouteURL: {RouteURL}", log.RequestId, log.RouteURL);
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating log entry");
                throw;
            }
        }

        public async Task<IEnumerable<Log>> GetLogsByRequestIdAsync(Guid requestId)
        {
            try
            {
                return await _dbContext.Logs
                    .Where(l => l.RequestId == requestId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs by RequestId: {RequestId}", requestId);
                throw;
            }
        }

        public async Task<IEnumerable<Log>> GetLogsByRouteURLAsync(string routeUrl)
        {
            try
            {
                return await _dbContext.Logs
                    .Where(l => l.RouteURL.Contains(routeUrl))
                    .OrderByDescending(l => l.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs by RouteURL: {RouteURL}", routeUrl);
                throw;
            }
        }

        public async Task<IEnumerable<Log>> GetLogsByDateRangeAsync(DateTime from, DateTime to, int page = 1, int pageSize = 50)
        {
            try
            {
                return await _dbContext.Logs
                    .Where(l => l.Timestamp >= from && l.Timestamp <= to)
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs by date range: {From} to {To}", from, to);
                throw;
            }
        }

        // New: Retrieves logs with multiple optional filters and pagination.
        public async Task<(IEnumerable<Log> Logs, int TotalCount)> GetLogsAsync(
            Guid? requestId = null,
            string? routeUrl = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                var query = _dbContext.Logs.AsQueryable();

                if (requestId.HasValue)
                {
                    query = query.Where(l => l.RequestId == requestId.Value);
                }

                if (!string.IsNullOrEmpty(routeUrl))
                {
                    query = query.Where(l => l.RouteURL.Contains(routeUrl));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(l => l.Timestamp <= toDate.Value);
                }

                var totalCount = await query.CountAsync();

                var logs = await query.OrderByDescending(l => l.Timestamp)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

                return (logs, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs with filters");
                throw;
            }
        }

        // New: Provides a queryable collection of logs for OData or similar purposes.
        public IQueryable<Log> GetLogsQueryable()
        {
            try
            {
                return _dbContext.Logs.AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving logs as queryable");
                throw;
            }
        }
    }
}
