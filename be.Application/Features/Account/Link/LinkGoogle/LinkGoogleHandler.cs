using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Entities;
using be.Domain.Enums;
using MediatR;

namespace be.Application.Features.Account.Link.LinkGoogle;

public class LinkGoogleHandler(
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IGoogle google,
    IAccountRepository accountRepository,
    IThirdPartyLoginsRepository thirdPartyLoginsRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LinkGoogleCommand, BaseResponse<bool>>
{
    public async Task<BaseResponse<bool>> Handle(LinkGoogleCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.UserId
                            ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var curendUser = await userRepository.GetUser2Async(currentUserId, cancellationToken)
                         ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var ggDto = await google.GetTokenAsync(request.Code, cancellationToken);
        var payload = await google.VerifyIdTokenAsync(ggDto.IdToken, cancellationToken);

        var hasConflict = await accountRepository.AnyAccountAsync(currentUserId, payload.Email, cancellationToken);

        if (hasConflict)
            throw new CustomException.BusinessValidationException("Email đã được sử dụng bởi tài khoản khác");

        var alreadyLinked = await thirdPartyLoginsRepository.AnyLinkAsync(curendUser.SequenceId, cancellationToken);

        if (alreadyLinked)
            throw new CustomException.BusinessValidationException("Tài khoản đã liên kết Google rồi");

        var thirdPartyLogin = new ThirdPartyLogin
        {
            Provider = ThirdPartyLoginEnum.google,
            ProviderId = payload.Subject,
            Mail = payload.Email,
            AccountId = curendUser.SequenceId
        };

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await accountRepository.UpdateThirdPartyAsync(curendUser.SequenceId, cancellationToken);

            await thirdPartyLoginsRepository.AddAsync(thirdPartyLogin, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
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
            Message = "Liên kết Google thành công"
        };
    }
}