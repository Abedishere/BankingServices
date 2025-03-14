using BankingServices.Models;

namespace BankingServices.Services
{
    public interface ILogService
    {
        // Store a new log entry in the database
        Task<Log> CreateLogAsync(Log log);
        
        // Get logs by RequestId
        Task<IEnumerable<Log>> GetLogsByRequestIdAsync(Guid requestId);
        
        // Get logs by RouteURL
        Task<IEnumerable<Log>> GetLogsByRouteURLAsync(string routeUrl);
        
        // Get logs by date range
        Task<IEnumerable<Log>> GetLogsByDateRangeAsync(DateTime from, DateTime to, int page = 1, int pageSize = 50);
        
        // Get logs with multiple filters
        Task<(IEnumerable<Log> Logs, int TotalCount)> GetLogsAsync(
            Guid? requestId = null, 
            string? routeUrl = null, 
            DateTime? fromDate = null, 
            DateTime? toDate = null, 
            int page = 1, 
            int pageSize = 50);
        
        // Get a queryable collection of logs for OData
        IQueryable<Log> GetLogsQueryable();
    }
}