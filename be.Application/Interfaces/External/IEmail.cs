using be.Application.Dtos.Shared;

namespace be.Application.Interfaces.External;

public interface IEmail
{
    Task<BaseResponse> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken);
}