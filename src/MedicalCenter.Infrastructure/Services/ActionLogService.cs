using MedicalCenter.Core.Services;
using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Core.Primitives.Pagination;
using MedicalCenter.Infrastructure.Data;
using MedicalCenter.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Queries;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Implementation of IActionLogger and IActionLogQueryService.
/// Enqueues entries for background processing (true fire-and-forget).
/// Provides query capabilities for action log history.
/// </summary>
public class ActionLogService(
    IActionLogQueue queue,
    MedicalCenterDbContext context,
    ILogger<ActionLogService> logger) : IActionLogger, IActionLogQueryService
{
    private readonly IActionLogQueue _queue = queue;
    private readonly MedicalCenterDbContext _context = context;
    private readonly ILogger<ActionLogService> _logger = logger;

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
        PaginationQuery<ActionLogQuery> query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ActionLogEntry> dbQuery = _context.Set<ActionLogEntry>();

        var criteria = query.Criteria;
        if (criteria != null)
        {
            dbQuery = dbQuery.Where(e => !criteria.StartDate.HasValue || e.ExecutedAt >= criteria.StartDate.Value)
                .Where(e => !criteria.EndDate.HasValue || e.ExecutedAt <= criteria.EndDate.Value)
                .Where(e => !criteria.UserId.HasValue || e.UserId == criteria.UserId.Value)
                .Where(e => string.IsNullOrWhiteSpace(criteria.ActionName) || e.ActionName == criteria.ActionName);
        }
        
        // Return paginated results using extension method
        return await dbQuery.OrderByDescending(e => e.ExecutedAt)
            .ToPaginatedListAsync(query.PageNumber, query.PageSize, cancellationToken);
    }
}

