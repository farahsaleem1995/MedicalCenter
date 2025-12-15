using MedicalCenter.Core.Repositories;
using MedicalCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MedicalCenter.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation using EF Core DbContext.
/// Manages database transactions and persistence operations.
/// </summary>
public class UnitOfWork(MedicalCenterDbContext context) : IUnitOfWork, IDisposable, IAsyncDisposable
{
    private readonly MedicalCenterDbContext _context = context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and any active transaction synchronously.
    /// If a transaction is active, it will be rolled back before disposal.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DisposeTransaction();
        _context.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void DisposeTransaction()
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            // Attempt to rollback if transaction is still active
            // Note: This is a best-effort rollback in synchronous context
            // ConfigureAwait(false) prevents deadlocks when called from DI container disposal
            // For proper async disposal, use DisposeAsync() instead
            _transaction.RollbackAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch
        {
            // Ignore rollback errors during disposal
        }
        finally
        {
            // ConfigureAwait(false) prevents deadlocks when called from DI container disposal
            _transaction.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the unit of work and any active transaction asynchronously.
    /// If a transaction is active, it will be rolled back before disposal.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await DisposeTransactionAsync().ConfigureAwait(false);
        await _context.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            // Rollback if transaction is still active
            // ConfigureAwait(false) prevents unnecessary context capture
            await _transaction.RollbackAsync().ConfigureAwait(false);
        }
        catch
        {
            // Ignore rollback errors during disposal - transaction may already be committed/disposed
        }
        finally
        {
            // Always dispose the transaction, even if rollback failed
            await _transaction.DisposeAsync().ConfigureAwait(false);
            _transaction = null;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(UnitOfWork));
        }
    }
}

