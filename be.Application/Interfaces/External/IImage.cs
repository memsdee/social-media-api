namespace be.Application.Interfaces.External;

public class ImageUploadCredentials
{
    public string Signature { get; set; } = null!;
    public string Exp { get; set; } = null!;
    public string PublicKey { get; set; } = null!;
}

public interface IImage
{
    Task<ImageUploadCredentials> SignatureAsync(CancellationToken cancellationToken);
    Task MoveImageAsync(List<Guid> fileId, CancellationToken cancellationToken);
    Task DeleteImageAsync(List<Guid> fileId, CancellationToken cancellationToken);
}