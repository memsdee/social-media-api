using System.Text.Json;
using be.Application.Dtos.EventBus;
using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Enums;
using MediatR;

namespace be.Application.Features.Notification.MarkRead;

public class MarkReadHandler(
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    INotificationRepository notificationRepository,
    IEncryption encryption,
    IOutboxRepository outboxRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<MarkReadCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(MarkReadCommand request, CancellationToken cancellationToken)
    {
        var myUserId = currentUserContext.UserId
                       ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại!");

        var userId = await userRepository.GetPrivateIdByPublicIdAsync(myUserId, cancellationToken)
                     ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại!");

        short[] notificationIds;
        try
        {
            var decrypted = encryption.Decrypt(request.EncryptedIds);
            notificationIds = JsonSerializer.Deserialize<short[]>(decrypted)
                              ?? throw new CustomException.BusinessValidationException("Dữ liệu không hợp lệ");
        }
        catch (Exception)
        {
            throw new CustomException.BusinessValidationException("Dữ liệu không hợp lệ");
        }

        if (notificationIds.Length == 0)
            throw new CustomException.BusinessValidationException("Dữ liệu không được để trống");

        var now = DateTimeOffset.UtcNow;

        var markReadNotiEvent = new MarkReadNotiEvent
        {
            NotificationIds = notificationIds,
            ReceiverSequenceId = userId,
            ReadAt = now
        };

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await notificationRepository.MarkReadAsync(notificationIds, userId, now, cancellationToken);
            await outboxRepository.AddAsync(OutboxTopicEnum.markAsReadNotification, markReadNotiEvent,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new BaseResponse
        {
            Message = "Đã đánh dấu đã đọc"
        };
    }
}