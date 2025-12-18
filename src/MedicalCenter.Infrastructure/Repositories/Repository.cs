using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using MedicalCenter.Core.Abstractions;
using MedicalCenter.Core.SharedKernel;
using MedicalCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenter.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation using EF Core and Ardalis.Specification.
/// Works only with aggregate roots (entities implementing IAggregateRoot).
/// </summary>
public class Repository<T> : RepositoryBase<T>, IRepository<T>
    where T : class, IAggregateRoot
{
    private readonly MedicalCenterDbContext _dbContext;

    public Repository(MedicalCenterDbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    async Task<T?> IRepository<T>.FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        return await base.FirstOrDefaultAsync(specification, cancellationToken);
    }

    async Task<IReadOnlyList<T>> IRepository<T>.ListAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        var list = await base.ListAsync(specification, cancellationToken);
        return list;
    }

    async Task<int> IRepository<T>.CountAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        return await base.CountAsync(specification, cancellationToken);
    }

    async Task<bool> IRepository<T>.AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken)
    {
        return await base.AnyAsync(specification, cancellationToken);
    }

    async Task<T> IRepository<T>.AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
        return entity;
    }

    Task IRepository<T>.UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        _dbContext.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    Task IRepository<T>.DeleteAsync(T entity, CancellationToken cancellationToken)
    {
        _dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }
}

