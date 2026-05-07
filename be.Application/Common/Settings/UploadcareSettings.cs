namespace be.Application.Common.Settings;

public class UploadcareSettings
{
    public string PublicKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public int ExpMinute { get; set; }
    public string UrlApi { get; set; } = null!;
    public string UrlImg { get; set; } = null!;
}