using be.Application.Dtos.Shared;
using be.Application.Interfaces.External;
using be.Infrastructure.Common.Appsetting;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace be.Infrastructure.Services;

public class SmtpService(IOptions<EmailSettings> emailSetting) : IEmail
{
    private readonly EmailSettings _emailSetting = emailSetting.Value;

    public async Task<BaseResponse> SendEmailAsync(string to, string subject, string body,
        CancellationToken cancellation)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_emailSetting.SenderName, _emailSetting.SenderEmail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(_emailSetting.SmtpServer, _emailSetting.SmtpPort, SecureSocketOptions.StartTls,
            cancellation);
        await smtp.AuthenticateAsync(_emailSetting.SmtpUser, _emailSetting.SmtpPassword, cancellation);

        await smtp.SendAsync(email, cancellation);
        await smtp.DisconnectAsync(true, cancellation);

        return new BaseResponse();
    }
}