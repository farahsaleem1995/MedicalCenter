using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MedicalCenter.Core.Aggregates.ActionLogs;
using MedicalCenter.Infrastructure.Options;

namespace MedicalCenter.Infrastructure.Services;

/// <summary>
/// Queue service for action log entries using BoundedChannel.
/// Provides thread-safe, bounded queue for action log processing.
/// </summary>
public class ActionLogQueue : IActionLogQueue
{
    private readonly Channel<ActionLogEntry> _channel;
    private readonly ILogger<ActionLogQueue> _logger;
    
    public ActionLogQueue(IOptions<ActionLogOptions> options, ILogger<ActionLogQueue> logger)
    {
        _logger = logger;
        
        // Create bounded channel with configurable capacity (default: 1000)
        int capacity = options.Value.QueueCapacity;
        BoundedChannelOptions channelOptions = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite, // Drop oldest when full (prevent blocking)
            SingleReader = true, // Single background service reader
            SingleWriter = false // Multiple writers (concurrent requests)
        };
        
        _channel = Channel.CreateBounded<ActionLogEntry>(channelOptions);
    }
    
    public bool TryEnqueue(ActionLogEntry entry)
    {
        try
        {
            return _channel.Writer.TryWrite(entry);
        }
        catch (Exception ex)
        {
            // Log but don't throw - action log failure shouldn't fail the request
            _logger.LogWarning(ex, "Failed to enqueue action log entry for action {ActionName}", entry.ActionName);
            return false;
        }
    }
    
    public async ValueTask<ActionLogEntry?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                if (_channel.Reader.TryRead(out ActionLogEntry? item))
                {
                    return item;
                }
            }
            
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}

