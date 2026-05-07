namespace be.Application.Dtos.Shared;

public record BaseResponse
{
    public string Message { get; init; } = string.Empty;
}

public record BaseResponse<T>
{
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
}