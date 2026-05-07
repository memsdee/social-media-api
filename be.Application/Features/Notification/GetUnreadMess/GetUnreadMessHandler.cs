using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using MediatR;

namespace be.Application.Features.Notification.GetUnreadMess;

public class GetUnreadMessHandler(
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IConversationReadRepository conversationReadRepository)
    : IRequestHandler<GetUnreadMessQuery, BaseResponse<int>>
{
    public async Task<BaseResponse<int>> Handle(GetUnreadMessQuery request, CancellationToken cancellationToken)
    {
        var myUserId = currentUserContext.UserId
                       ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var userId = await userRepository.GetPrivateIdByPublicIdAsync(myUserId, cancellationToken)
                     ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var totalUnread = await conversationReadRepository.GetTotalUnreadCountAsync(userId, cancellationToken);

        return new BaseResponse<int>
        {
            Data = totalUnread
        };
    }
}