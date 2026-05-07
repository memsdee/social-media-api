using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Notification.GetUnreadMess;

public class GetUnreadMessQuery : IRequest<BaseResponse<int>>
{
}