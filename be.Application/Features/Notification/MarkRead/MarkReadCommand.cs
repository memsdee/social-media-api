using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Notification.MarkRead;

public class MarkReadCommand : IRequest<BaseResponse>
{
    public string EncryptedIds { get; set; } = null!;
}