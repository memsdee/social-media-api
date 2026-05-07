namespace be.Application.Dtos.Pagination;

public record CursorResult<T, TKey>(
    IReadOnlyList<T> Items,
    bool HasNextPage,
    TKey? NextCursor
);