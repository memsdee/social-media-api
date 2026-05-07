using be.Application.Common.Settings;
using be.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Services;

public class FormatService(
    IOptions<UploadcareSettings> uploadcare,
    IOptions<DefaultInfoSettings> defaultInfo) : IFormat
{
    private readonly Guid _defaultAvatar = defaultInfo.Value.Avatar;
    private readonly string _uploadcareBaseUrl = uploadcare.Value.UrlImg;

    public string FormatImageUrl(Guid? avatar, string userId)
    {
        return $"{_uploadcareBaseUrl}{avatar ?? _defaultAvatar}/{userId}.webp";
    }

    public string? FormatThumbnailNotiUrl(Guid? imageId)
    {
        return imageId.HasValue ? $"{_uploadcareBaseUrl}{imageId}/daily.webp" : null;
    }

    public string? FormatNotiPreview(string? content, int maxLength = 67)
    {
        if (string.IsNullOrEmpty(content)) return null;
        return content.Length > maxLength
            ? string.Concat(content.AsSpan(0, maxLength), "...")
            : content;
    }
}