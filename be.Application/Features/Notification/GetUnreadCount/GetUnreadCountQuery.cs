using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Notification.GetUnreadCount;

public class GetUnreadCountQuery : IRequest<BaseResponse<GetUnreadCountResponse>>
{
}