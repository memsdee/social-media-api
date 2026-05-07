using System.Text;
using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace be.Application.Features.Account.Link.LinkPass;

public class LinkPassHandler(
    IDistributedCache cache,
    ICurrentUserContext currentUserContext,
    IAccountRepository accountRepository,
    IThirdPartyLoginsRepository thirdPartyLoginsRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<LinkPassCommand, BaseResponse>
{
    public async Task<BaseResponse> Handle(LinkPassCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.UserId
                            ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var currentAccount = await accountRepository.GetAccount6Async(currentUserId, cancellationToken)
                             ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        if (!string.IsNullOrEmpty(currentAccount.Pass))
            throw new CustomException.BusinessValidationException("Tài khoản đã liên kết mật khẩu rồi");

        var cacheOtpBytes = await cache.GetAsync(request.Mail, cancellationToken);
        var cacheOtp = cacheOtpBytes != null ? Encoding.UTF8.GetString(cacheOtpBytes) : null;
        if (cacheOtp is null || cacheOtp != request.Otp)
            throw new CustomException.BusinessValidationException("OTP không đúng hoặc đã hết hạn");

        var hasAccountMailConflict = await
            accountRepository.AnyAccountMailAsync(currentAccount.PrivateAccountId, request.Mail, cancellationToken);

        if (hasAccountMailConflict)
            throw new CustomException.BusinessValidationException("Email đã được sử dụng bởi tài khoản khác");

        var hasThirdPartyMailConflict = await
            thirdPartyLoginsRepository.AnyThirdPartyMailAsync(currentAccount.PrivateAccountId, request.Mail,
                cancellationToken);

        if (hasThirdPartyMailConflict)
            throw new CustomException.BusinessValidationException("Email đã được sử dụng bởi tài khoản khác");

        var affected = await
            accountRepository.UpdateAccountAsync(currentAccount.PrivateAccountId, request.Mail,
                passwordHasher.Hash(request.Pass), cancellationToken);

        if (affected != 1)
            throw new CustomException.BusinessValidationException("Có lỗi xảy ra, vui lòng thử lại");

        return new BaseResponse
        {
            Message = "Liên kết mail và mật khẩu thành công"
        };
    }
}