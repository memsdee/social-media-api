namespace be.Application.Dtos.Pagination;

public class PagiResult
{
    public bool HasNextPage { get; set; }
    public string? NextCursor { get; set; }
}