using MedicalCenter.Core.Services;
using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Options;
using MedicalCenter.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IActionLogService.
/// Enqueues entries for background processing (true fire-and-forget).
/// </summary>
public class ActionLogService : IActionLogService
{
    private readonly IActionLogQueue _queue;
    private readonly MedicalCenterDbContext _context;
    private readonly ILogger<ActionLogService> _logger;
    
    public ActionLogService(
        IActionLogQueue queue,
        MedicalCenterDbContext context,
        ILogger<ActionLogService> logger)
    {
        _queue = queue;
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Records an action log entry for background processing.
    /// Non-blocking, synchronous operation (like logger pattern).
    /// </summary>
    public void Record(ActionLogEntry entry)
    {
        if (!_queue.TryEnqueue(entry))
        {
            // Queue is full - log warning but don't fail request
            _logger.LogWarning(
                "Action log queue is full, dropping entry for action {ActionName}",
                entry.ActionName);
        }
    }
    
    /// <summary>
    /// Retrieves paginated action log history with filtering, ordering, and pagination.
    /// </summary>
    public async Task<PaginatedList<ActionLogEntry>> GetHistory(
        ActionLogQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ActionLogEntry> dbQuery = _context.Set<ActionLogEntry>()
            .Where(e => !query.StartDate.HasValue || e.ExecutedAt >= query.StartDate.Value)
            .Where(e => !query.EndDate.HasValue || e.ExecutedAt <= query.EndDate.Value)
            .Where(e => !query.UserId.HasValue || e.UserId == query.UserId.Value)
            .Where(e => string.IsNullOrWhiteSpace(query.ActionName) || e.ActionName == query.ActionName)
            .OrderByDescending(e => e.ExecutedAt);
        
        // Return paginated results using extension method
        return await dbQuery.ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }
}

