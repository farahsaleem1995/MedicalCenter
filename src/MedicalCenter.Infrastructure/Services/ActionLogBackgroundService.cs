using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Infrastructure.Data;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Background service that processes action log entries from the queue.
/// Creates a scope for each entry to resolve DbContext independently.
/// </summary>
public class ActionLogBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IActionLogQueue _queue;
    private readonly ILogger<ActionLogBackgroundService> _logger;
    
    public ActionLogBackgroundService(
        IServiceProvider serviceProvider,
        IActionLogQueue queue,
        ILogger<ActionLogBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _queue = queue;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Action log background service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Dequeue entry (waits if queue is empty)
                ActionLogEntry? item = await _queue.DequeueAsync(stoppingToken);
                
                if (item == null)
                    continue;
                
                // Process entry in its own scope
                await ProcessEntryAsync(item, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing action log entry");
                // Continue processing - don't let one error stop the service
            }
        }
        
        _logger.LogInformation("Action log background service stopped");
    }
    
    private async Task ProcessEntryAsync(ActionLogEntry entry, CancellationToken cancellationToken)
    {
        // Create scope for this entry (independent DbContext)
        using IServiceScope scope = _serviceProvider.CreateScope();
        MedicalCenterDbContext context = scope.ServiceProvider.GetRequiredService<MedicalCenterDbContext>();
        
        try
        {
            // Entry is already created, just add and save
            context.Set<ActionLogEntry>().Add(entry);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to save action log entry for action {ActionName}",
                entry.ActionName);
            // Don't throw - continue processing other entries
        }
    }
}

