using MedicalCenter.Core.Common;
using MedicalCenter.Core.Enums;
using MedicalCenter.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalCenter.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static PaginatedList<T> ToPaginatedList<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return new PaginatedList<T>(
            [.. query.Skip((pageNumber - 1) * pageSize).Take(pageSize)],
            pageNumber,
            pageSize,
            query.Count());
    }

    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return new PaginatedList<T>(
            [.. await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken)],
            pageNumber,
            pageSize,
            await query.CountAsync(cancellationToken));
    }
}