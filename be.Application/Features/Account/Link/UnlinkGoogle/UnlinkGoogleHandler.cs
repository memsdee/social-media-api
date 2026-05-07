using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using MediatR;

namespace be.Application.Features.Account.Link.UnlinkGoogle;

public class UnlinkGoogleHandler(
    ICurrentUserContext currentUserContext,
    IAccountRepository accountRepository,
    IThirdPartyLoginsRepository thirdPartyLoginsRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UnlinkGoogleCommand, BaseResponse<bool>>
{
    public async Task<BaseResponse<bool>> Handle(UnlinkGoogleCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.UserId
                            ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var account = await accountRepository.GetAccount7Async(currentUserId, cancellationToken)
                      ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var thirdPartyState =
            await thirdPartyLoginsRepository.GetThirdParty2Async(account.PrivateAccountId, cancellationToken);

        if (thirdPartyState is null || !thirdPartyState.HasGoogle)
            throw new CustomException.BusinessValidationException("Tài khoản chưa liên kết Google");

        if (!thirdPartyState.HasOtherThirdParty && !account.HasPass)
            throw new CustomException.BusinessValidationException(
                "Tài khoản chỉ liên kết Google và không có mật khẩu, không thể gỡ liên kết");

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await thirdPartyLoginsRepository.DelAsync(account.PrivateAccountId, cancellationToken);

            await accountRepository.UpdateThirdPartyAsync(account.PrivateAccountId, thirdPartyState.HasOtherThirdParty,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new BaseResponse<bool>
        {
            Data = true,
            Message = "Gỡ liên kết Google thành công"
        };
    }
}