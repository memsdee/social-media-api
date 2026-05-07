using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using MediatR;

namespace be.Application.Features.Notification.GetUnreadCount;

public class GetUnreadCountHandler(
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    INotificationReadRepository notificationReadRepository)
    : IRequestHandler<GetUnreadCountQuery, BaseResponse<GetUnreadCountResponse>>
{
    public async Task<BaseResponse<GetUnreadCountResponse>> Handle(GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        var myUserId = currentUserContext.UserId
                       ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại!");

        var userId = await userRepository.GetPrivateIdByPublicIdAsync(myUserId, cancellationToken)
                     ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại!");

        var count = await notificationReadRepository.GetUnreadCountAsync(userId, cancellationToken);

        if (count >= 99)
            count = 99;

        return new BaseResponse<GetUnreadCountResponse>
        {
            Data = new GetUnreadCountResponse
            {
                Count = (int)count
            }
        };
    }
}