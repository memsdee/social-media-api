using be.Application.Dtos.Queries.User;
using be.Application.Dtos.Shared;
using be.Application.Features.Account.Change.EditAccount;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Entities;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Account.Change.EditAccount;

public class EditAccountHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly EditAccountHandler _handler;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUseridChangeLogsRepository _useridChangeLogsRepository;
    private readonly IUsernameChangeLogsRepository _usernameChangeLogsRepository;
    private readonly IUserRepository _userRepository;

    public EditAccountHandlerTests()
    {
        _tokenGenerator = Substitute.For<ITokenGenerator>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _userRepository = Substitute.For<IUserRepository>();
        _usernameChangeLogsRepository = Substitute.For<IUsernameChangeLogsRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _useridChangeLogsRepository = Substitute.For<IUseridChangeLogsRepository>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new EditAccountHandler(
            _tokenGenerator,
            _currentUserContext,
            _userRepository,
            _usernameChangeLogsRepository,
            _unitOfWork,
            _useridChangeLogsRepository);
    }

    [Fact]
    public async Task Handle_UserNotLoggedIn_ThrowsUnauthorizedException()
    {
        _currentUserContext.UserId.ReturnsNull();
        var command = new EditAccountCommand();

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUserByPublicIdAsync("user-id", Arg.Any<CancellationToken>()).ReturnsNull();
        var command = new EditAccountCommand();

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Người dùng không tồn tại");
    }

    [Fact]
    public async Task Handle_UserNameSameAsOld_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUserByPublicIdAsync("user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "old_name", PublicUserId = "user-id" });
        var command = new EditAccountCommand { UserName = "old_name" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Tên mới không được giống tên cũ");
    }

    [Fact]
    public async Task Handle_UserNameChangedRecently_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUserByPublicIdAsync("user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "old_name", PublicUserId = "user-id" });
        var logTime = DateTimeOffset.UtcNow.AddDays(-6);
        _usernameChangeLogsRepository.GetCreateAtByPritaveUserId(1, Arg.Any<CancellationToken>()).Returns(logTime);
        var command = new EditAccountCommand { UserName = "new_name" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException<object>>()
            .WithMessage("1 tuần chỉ đổi tên được 1 lần");
    }

    [Fact]
    public async Task Handle_UserIdSameAsOld_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUserByPublicIdAsync("user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "name", PublicUserId = "user-id" });
        var command = new EditAccountCommand { UserId = "user-id" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Id mới không được giống Id cũ");
    }

    [Fact]
    public async Task Handle_UserIdExists_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUserByPublicIdAsync("user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "name", PublicUserId = "user-id" });
        _userRepository.ExistsAsync("existing-id", Arg.Any<CancellationToken>()).Returns(true);
        var command = new EditAccountCommand { UserId = "existing-id" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Id này đã tồn tại, vui lòng chọn Id khác");
    }

    [Fact]
    public async Task Handle_UserIdChangedRecently_ThrowsBusinessValidationException()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUserByPublicIdAsync("user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "name", PublicUserId = "user-id" });
        _userRepository.ExistsAsync("new-id", Arg.Any<CancellationToken>()).Returns(false);
        var logTime = DateTimeOffset.UtcNow.AddDays(-29);
        _useridChangeLogsRepository.GetCreateAtByPublicUserId(1, Arg.Any<CancellationToken>()).Returns(logTime);
        var command = new EditAccountCommand { UserId = "new-id" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException<object>>()
            .WithMessage("1 tháng chỉ đổi Id được 1 lần");
    }

    [Fact]
    public async Task Handle_ValidRequest_Success()
    {
        _currentUserContext.UserId.Returns("old-user-id");
        _userRepository.GetUserByPublicIdAsync("old-user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "old_name", PublicUserId = "old-user-id", Role = RoleEnum.user });
        _userRepository.ExistsAsync("new-user-id", Arg.Any<CancellationToken>()).Returns(false);
        _usernameChangeLogsRepository.GetCreateAtByPritaveUserId(1, Arg.Any<CancellationToken>()).ReturnsNull();
        _useridChangeLogsRepository.GetCreateAtByPublicUserId(1, Arg.Any<CancellationToken>()).ReturnsNull();

        _tokenGenerator.CreateAccessTokensAsync("new-user-id", RoleEnum.user)
            .Returns(new BaseResponse<string> { Data = "access_token" });
        _tokenGenerator.CreateRefreshTokenAsync(1, Arg.Any<CancellationToken>())
            .Returns(new BaseResponse<string> { Data = "refresh_token" });

        var command = new EditAccountCommand { UserName = "new_name", UserId = "new-user-id", Bio = "new_bio" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Message.Should().Be("Cập nhật thông tin tài khoản thành công");
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("access_token");
        result.Data.RefreshToken.Should().Be("refresh_token");

        await _userRepository.Received(1).UpdateNameAsync("new_name", 1, Arg.Any<CancellationToken>());
        await _usernameChangeLogsRepository.Received(1).AddAsync(
            Arg.Is<UsernameChangeLog>(x => x.NewUsername == "new_name" && x.UserId == 1), Arg.Any<CancellationToken>());

        await _userRepository.Received(1).UpdateUserIdAsync(1, "new-user-id", Arg.Any<CancellationToken>());
        await _useridChangeLogsRepository.Received(1)
            .AddAsync(Arg.Is<UseridChangeLog>(x => x.NewUserId == "new-user-id" && x.UserId == 1),
                Arg.Any<CancellationToken>());

        await _userRepository.Received(1).UpdateBioAsync(1, "new_bio", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidRequestNoChanges_GeneratesTokenWithOldId()
    {
        _currentUserContext.UserId.Returns("old-user-id");
        _userRepository.GetUserByPublicIdAsync("old-user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "old_name", PublicUserId = "old-user-id", Role = RoleEnum.user });

        _tokenGenerator.CreateAccessTokensAsync("old-user-id", RoleEnum.user)
            .Returns(new BaseResponse<string> { Data = "access_token" });
        _tokenGenerator.CreateRefreshTokenAsync(1, Arg.Any<CancellationToken>())
            .Returns(new BaseResponse<string> { Data = "refresh_token" });

        var command = new EditAccountCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Message.Should().Be("Cập nhật thông tin tài khoản thành công");
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TransactionRollback_WhenExceptionOccursDuringUpdate()
    {
        _currentUserContext.UserId.Returns("user-id");
        _userRepository.GetUserByPublicIdAsync("user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "old_name", PublicUserId = "user-id" });
        _usernameChangeLogsRepository.GetCreateAtByPritaveUserId(1, Arg.Any<CancellationToken>()).ReturnsNull();

        _userRepository.UpdateNameAsync("new_name", 1, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Database error")));

        var command = new EditAccountCommand { UserName = "new_name" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TokenGenerationFails_ThrowsException()
    {
        _currentUserContext.UserId.Returns("old-user-id");
        _userRepository.GetUserByPublicIdAsync("old-user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "old_name", PublicUserId = "old-user-id", Role = RoleEnum.user });

        _tokenGenerator.CreateAccessTokensAsync("old-user-id", RoleEnum.user)
            .Returns(new BaseResponse<string> { Data = null });
        _tokenGenerator.CreateRefreshTokenAsync(1, Arg.Any<CancellationToken>())
            .Returns(new BaseResponse<string> { Data = "refresh_token" });

        var command = new EditAccountCommand();

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Có lỗi xảy ra, vui lòng liên hệ quản trị viên");
    }

    [Fact]
    public async Task Handle_OnlyUserIdChanged_GeneratesTokenWithNewId()
    {
        _currentUserContext.UserId.Returns("old-user-id");
        _userRepository.GetUserByPublicIdAsync("old-user-id", Arg.Any<CancellationToken>())
            .Returns(new User1Dto { Id = 1, Name = "old_name", PublicUserId = "old-user-id", Role = RoleEnum.user });
        _userRepository.ExistsAsync("new-user-id", Arg.Any<CancellationToken>()).Returns(false);
        _useridChangeLogsRepository.GetCreateAtByPublicUserId(1, Arg.Any<CancellationToken>()).ReturnsNull();

        _tokenGenerator.CreateAccessTokensAsync("new-user-id", RoleEnum.user)
            .Returns(new BaseResponse<string> { Data = "access_token" });
        _tokenGenerator.CreateRefreshTokenAsync(1, Arg.Any<CancellationToken>())
            .Returns(new BaseResponse<string> { Data = "refresh_token" });

        var command = new EditAccountCommand { UserId = "new-user-id" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Message.Should().Be("Cập nhật thông tin tài khoản thành công");
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("access_token");
        result.Data.RefreshToken.Should().Be("refresh_token");

        await _userRepository.Received(1).UpdateUserIdAsync(1, "new-user-id", Arg.Any<CancellationToken>());
        await _tokenGenerator.Received(1).CreateAccessTokensAsync("new-user-id", RoleEnum.user);
    }
}