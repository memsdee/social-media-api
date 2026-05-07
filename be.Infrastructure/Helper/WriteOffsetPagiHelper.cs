using System.Linq.Expressions;
using be.Application.Dtos.Pagination;

namespace be.Infrastructure.Helper;

public static class WriteOffsetPagiHelper
{
    public static IQueryable<T> Apply<T, TKey>(IQueryable<T> query, int page, int pageSize,
        Expression<Func<T, TKey>> orderBy)
    {
        return query.OrderByDescending(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    public static OffsetPageInfo Calculate(int totalItems, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new OffsetPageInfo
        {
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}