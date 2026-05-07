namespace be.Application.Dtos.Pagination;

public record CursorPayload<TKey>(TKey Selector, short Id);