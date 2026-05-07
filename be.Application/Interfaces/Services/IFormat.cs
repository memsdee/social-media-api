namespace be.Application.Interfaces.Services;

public interface IFormat
{
    string FormatImageUrl(Guid? avatar, string userId);
    string? FormatThumbnailNotiUrl(Guid? imageId);
    string? FormatNotiPreview(string? content, int maxLength = 67);
}