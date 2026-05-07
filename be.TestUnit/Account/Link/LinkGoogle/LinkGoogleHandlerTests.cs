using be.Application.Dtos.OAuth;
using be.Application.Dtos.Queries.User;
using be.Application.Features.Account.Link.LinkGoogle;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.Link.LinkGoogle;

public class LinkGoogleHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGoogle _google;
    private readonly LinkGoogleHandler _handler;
    private readonly IThirdPartyLoginsRepository _thirdPartyLoginsRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public LinkGoogleHandlerTests()
    {
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _google = Substitute.For<IGoogle>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _thirdPartyLoginsRepository = Substitute.For<IThirdPartyLoginsRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new LinkGoogleHandler(
            _currentUserContext,
            _userRepository,
            _google,
            _accountRepository,
            _thirdPartyLoginsRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        var command = new LinkGoogleCommand { Code = "code" };
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        var command = new LinkGoogleCommand { Code = "code" };
        var userId = "userId";
        _currentUserContext.UserId.Returns(userId);
        _userRepository.GetUser2Async(userId, Arg.Any<CancellationToken>()).ReturnsNull();

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenEmailInConflict_ShouldThrowBusinessValidationException()
    {
        var command = new LinkGoogleCommand { Code = "code" };
        var userId = "userId";
        var userDto = new User2Dto { SequenceId = 1 };
        var googleDto = new GoogleDto { IdToken = "idToken" };
        var googleUserInfo = new GoogleUserInfoDto { Email = "conflict@mail.com", Subject = "subject" };

        _currentUserContext.UserId.Returns(userId);
        _userRepository.GetUser2Async(userId, Arg.Any<CancellationToken>()).Returns(userDto);
        _google.GetTokenAsync(command.Code, Arg.Any<CancellationToken>()).Returns(googleDto);
        _google.VerifyIdTokenAsync(googleDto.IdToken, Arg.Any<CancellationToken>()).Returns(googleUserInfo);
        _accountRepository.AnyAccountAsync(userId, googleUserInfo.Email, Arg.Any<CancellationToken>()).Returns(true);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Email đã được sử dụng bởi tài khoản khác");
    }

    [Fact]
    public async Task Handle_WhenAlreadyLinked_ShouldThrowBusinessValidationException()
    {
        var command = new LinkGoogleCommand { Code = "code" };
        var userId = "userId";
        var userDto = new User2Dto { SequenceId = 1 };
        var googleDto = new GoogleDto { IdToken = "idToken" };
        var googleUserInfo = new GoogleUserInfoDto { Email = "test@mail.com", Subject = "subject" };

        _currentUserContext.UserId.Returns(userId);
        _userRepository.GetUser2Async(userId, Arg.Any<CancellationToken>()).Returns(userDto);
        _google.GetTokenAsync(command.Code, Arg.Any<CancellationToken>()).Returns(googleDto);
        _google.VerifyIdTokenAsync(googleDto.IdToken, Arg.Any<CancellationToken>()).Returns(googleUserInfo);
        _accountRepository.AnyAccountAsync(userId, googleUserInfo.Email, Arg.Any<CancellationToken>()).Returns(false);
        _thirdPartyLoginsRepository.AnyLinkAsync(userDto.SequenceId, Arg.Any<CancellationToken>()).Returns(true);

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tài khoản đã liên kết Google rồi");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldCommitTransactionAndReturnTrue()
    {
        var command = new LinkGoogleCommand { Code = "code" };
        var userId = "userId";
        var userDto = new User2Dto { SequenceId = 1 };
        var googleDto = new GoogleDto { IdToken = "idToken" };
        var googleUserInfo = new GoogleUserInfoDto { Email = "test@mail.com", Subject = "subject" };

        _currentUserContext.UserId.Returns(userId);
        _userRepository.GetUser2Async(userId, Arg.Any<CancellationToken>()).Returns(userDto);
        _google.GetTokenAsync(command.Code, Arg.Any<CancellationToken>()).Returns(googleDto);
        _google.VerifyIdTokenAsync(googleDto.IdToken, Arg.Any<CancellationToken>()).Returns(googleUserInfo);
        _accountRepository.AnyAccountAsync(userId, googleUserInfo.Email, Arg.Any<CancellationToken>()).Returns(false);
        _thirdPartyLoginsRepository.AnyLinkAsync(userDto.SequenceId, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Data.Should().BeTrue();
        result.Message.Should().Be("Liên kết Google thành công");
        await _accountRepository.Received(1).UpdateThirdPartyAsync(userDto.SequenceId, Arg.Any<CancellationToken>());
        await _thirdPartyLoginsRepository.Received(1)
            .AddAsync(Arg.Any<ThirdPartyLogin>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollbackTransaction()
    {
        var command = new LinkGoogleCommand { Code = "code" };
        var userId = "userId";
        var userDto = new User2Dto { SequenceId = 1 };
        var googleDto = new GoogleDto { IdToken = "idToken" };
        var googleUserInfo = new GoogleUserInfoDto { Email = "test@mail.com", Subject = "subject" };

        _currentUserContext.UserId.Returns(userId);
        _userRepository.GetUser2Async(userId, Arg.Any<CancellationToken>()).Returns(userDto);
        _google.GetTokenAsync(command.Code, Arg.Any<CancellationToken>()).Returns(googleDto);
        _google.VerifyIdTokenAsync(googleDto.IdToken, Arg.Any<CancellationToken>()).Returns(googleUserInfo);
        _accountRepository.AnyAccountAsync(userId, googleUserInfo.Email, Arg.Any<CancellationToken>()).Returns(false);
        _thirdPartyLoginsRepository.AnyLinkAsync(userDto.SequenceId, Arg.Any<CancellationToken>()).Returns(false);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Database error"));

        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }
}