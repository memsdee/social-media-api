using be.Application.Dtos.Pagination;

namespace be.Infrastructure.Helper;

public static class ReadCursorPagiCaculHelper
{
    public static CursorResult<TDto, TKey?> Paginate<TDto, TKey>(
        List<TDto> items,
        int pageSize,
        Func<TDto, TKey> getKey
    )
        where TKey : notnull
    {
        var hasNext = items.Count > pageSize;
        if (hasNext) items.RemoveAt(items.Count - 1);

        return new CursorResult<TDto, TKey?>(
            items,
            hasNext,
            hasNext && items.Count > 0 ? getKey(items[^1]) : default
        );
    }
}