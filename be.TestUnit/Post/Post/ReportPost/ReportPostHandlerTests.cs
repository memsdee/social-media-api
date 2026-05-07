using be.Application.Dtos.EventBus;
using be.Application.Dtos.Queries.User;
using be.Application.Features.Post.Post.ReportPost;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Domain;
using be.Domain.Entities;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.Post.Post.ReportPost;

public class ReportPostHandlerTests
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ReportPostHandler _handler;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IPostRepository _postRepository;
    private readonly IReportPostRepository _reportPostRepository;
    private readonly ITransaction _transaction;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public ReportPostHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _reportPostRepository = Substitute.For<IReportPostRepository>();
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserContext = Substitute.For<ICurrentUserContext>();
        _transaction = Substitute.For<ITransaction>();

        _unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);

        _handler = new ReportPostHandler(
            _userRepository,
            _postRepository,
            _reportPostRepository,
            _outboxRepository,
            _unitOfWork,
            _currentUserContext);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns((string?)null);

        Func<Task> act = async () => await _handler.Handle(new ReportPostCommand
        {
            PostId = Guid.NewGuid(),
            ReasonId = [1]
        }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenReporterInfoMissing_ShouldThrowUnauthorizedException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetReportUserInfoAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns((ReportUserInfoDto?)null);

        Func<Task> act = async () => await _handler.Handle(new ReportPostCommand
        {
            PostId = Guid.NewGuid(),
            ReasonId = [1]
        }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.UnauthorizedException>()
            .WithMessage("Vui lòng đăng nhập lại");
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldThrowNotFoundException()
    {
        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetReportUserInfoAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(new ReportUserInfoDto { Id = 1, PublicId = "user-public-id", Name = "User", Email = "a@b.com" });
        _postRepository.GetPostByIdPublicAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((be.Domain.Entities.Post?)null);

        Func<Task> act = async () => await _handler.Handle(new ReportPostCommand
        {
            PostId = Guid.NewGuid(),
            ReasonId = [1]
        }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Bài đăng không tồn tại");
    }

    [Fact]
    public async Task Handle_WhenReasonCountMismatch_ShouldThrowNotFoundException()
    {
        var postId = Guid.NewGuid();
        var reasons = new List<short> { 1, 2 };

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetReportUserInfoAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(new ReportUserInfoDto { Id = 1, PublicId = "user-public-id", Name = "User", Email = "a@b.com" });
        _postRepository.GetPostByIdPublicAsync(postId, Arg.Any<CancellationToken>())
            .Returns(new be.Domain.Entities.Post { Id = 10, IdPublic = postId });
        _reportPostRepository.CountReasonReportPostAsync(reasons, Arg.Any<CancellationToken>())
            .Returns(1);

        Func<Task> act = async () => await _handler.Handle(new ReportPostCommand
        {
            PostId = postId,
            ReasonId = reasons
        }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage("Lý do báo cáo không tồn tại");
    }

    [Fact]
    public async Task Handle_WhenAlreadyReported_ShouldThrowBusinessValidationException()
    {
        var postId = Guid.NewGuid();
        var reasons = new List<short> { 1, 2 };

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetReportUserInfoAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(new ReportUserInfoDto { Id = 1, PublicId = "user-public-id", Name = "User", Email = "a@b.com" });
        _postRepository.GetPostByIdPublicAsync(postId, Arg.Any<CancellationToken>())
            .Returns(new be.Domain.Entities.Post { Id = 10, IdPublic = postId });
        _reportPostRepository.CountReasonReportPostAsync(reasons, Arg.Any<CancellationToken>())
            .Returns(reasons.Count);
        _reportPostRepository.GetReasonMapAsync(reasons, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, short> { { 1, 11 }, { 2, 22 } });
        _reportPostRepository.GetOtherReasonCodeAsync(Arg.Any<CancellationToken>())
            .Returns((short?)null);
        _reportPostRepository.HasAlreadyReportedAsync(1, 10, Arg.Any<List<short>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Func<Task> act = async () => await _handler.Handle(new ReportPostCommand
        {
            PostId = postId,
            ReasonId = reasons
        }, CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.BusinessValidationException>()
            .WithMessage("Bạn đã báo cáo bài đăng với lý do này rồi!");
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldCreateReportsAndCommit()
    {
        var postId = Guid.NewGuid();
        var reasons = new List<short> { 1, 2 };
        var otherReasonCode = (short)2;

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetReportUserInfoAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(new ReportUserInfoDto
            {
                Id = 1,
                PublicId = "user-public-id",
                Name = "User",
                Email = "a@b.com",
                Avatar = Guid.NewGuid()
            });
        _postRepository.GetPostByIdPublicAsync(postId, Arg.Any<CancellationToken>())
            .Returns(new be.Domain.Entities.Post { Id = 10, IdPublic = postId });
        _reportPostRepository.CountReasonReportPostAsync(reasons, Arg.Any<CancellationToken>())
            .Returns(reasons.Count);
        _reportPostRepository.GetReasonMapAsync(reasons, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, short> { { 1, 11 }, { 2, 22 } });
        _reportPostRepository.GetOtherReasonCodeAsync(Arg.Any<CancellationToken>())
            .Returns(otherReasonCode);
        _reportPostRepository.HasAlreadyReportedAsync(1, 10, Arg.Any<List<short>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new ReportPostCommand
        {
            PostId = postId,
            ReasonId = reasons,
            OtherReason = "Other details"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Message.Should().Be("Báo cáo bài đăng thành công");
        await _reportPostRepository.Received(1).AddUserReportsAsync(
            Arg.Is<List<UserReportPost>>(x => x.Count == 2 && x.Any(r => r.OtherReason == "Other details")),
            Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).AddAsync(OutboxTopicEnum.reportPost, Arg.Any<ReportPostEvent>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldRollback()
    {
        var postId = Guid.NewGuid();
        var reasons = new List<short> { 1 };

        _currentUserContext.UserId.Returns("user-public-id");
        _userRepository.GetReportUserInfoAsync("user-public-id", Arg.Any<CancellationToken>())
            .Returns(new ReportUserInfoDto { Id = 1, PublicId = "user-public-id", Name = "User", Email = "a@b.com" });
        _postRepository.GetPostByIdPublicAsync(postId, Arg.Any<CancellationToken>())
            .Returns(new be.Domain.Entities.Post { Id = 10, IdPublic = postId });
        _reportPostRepository.CountReasonReportPostAsync(reasons, Arg.Any<CancellationToken>())
            .Returns(reasons.Count);
        _reportPostRepository.GetReasonMapAsync(reasons, Arg.Any<CancellationToken>())
            .Returns(new Dictionary<short, short> { { 1, 11 } });
        _reportPostRepository.GetOtherReasonCodeAsync(Arg.Any<CancellationToken>())
            .Returns((short?)null);
        _reportPostRepository.HasAlreadyReportedAsync(1, 10, Arg.Any<List<short>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _reportPostRepository.AddUserReportsAsync(Arg.Any<List<UserReportPost>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("db error")));

        Func<Task> act = async () => await _handler.Handle(new ReportPostCommand
        {
            PostId = postId,
            ReasonId = reasons
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("db error");
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}