using System.Text.Json;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Application.Features.User.GetListImage;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Enums;
using FluentAssertions;
using NSubstitute;

namespace Tests.User.GetListImage;

public class GetListImageHandlerTests
{
    private readonly IEncryption _encryption;
    private readonly IFormat _format;
    private readonly GetListImageHandler _handler;
    private readonly IPostReadRepository _postReadRepository;
    private readonly IUserRepository _userRepository;

    public GetListImageHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _encryption = Substitute.For<IEncryption>();
        _postReadRepository = Substitute.For<IPostReadRepository>();
        _format = Substitute.For<IFormat>();

        _handler = new GetListImageHandler(
            _userRepository,
            _encryption,
            _postReadRepository,
            _format);
    }

    [Fact]
    public async Task Handle_WhenTargetUserNotFound_ShouldThrowNotFoundException()
    {
        var targetId = "non_existent";
        _userRepository.GetPrivateIdByPublicIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns((short?)null);

        Func<Task> act = async () =>
            await _handler.Handle(new GetListImageQuery(targetId, null, 10), CancellationToken.None);

        await act.Should().ThrowAsync<CustomException.NotFoundException>()
            .WithMessage($"Không tồn tại tài khoản '{targetId}'");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnImages()
    {
        var targetId = "target_public";
        var targetPrivateId = (short)10;
        var postPublicId = Guid.NewGuid();
        var imgId = Guid.NewGuid();

        var postImages = new List<PostImageDto>
        {
            new()
            {
                PublicId = postPublicId,
                Images = [new PostImages { Image = imgId, Type = ImageEnum.normal }]
            }
        };
        var cursorResult = new CursorResult<PostImageDto, CursorPayload<DateTimeOffset>?>(postImages, false, null);

        _userRepository.GetPrivateIdByPublicIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns(targetPrivateId);
        _postReadRepository.GetPostImageAsync(targetPrivateId, 10, null, Arg.Any<CancellationToken>())
            .Returns(cursorResult);
        _format.FormatImageUrl(imgId, targetId).Returns("formatted-url");

        var result = await _handler.Handle(new GetListImageQuery(targetId, null, 10), CancellationToken.None);

        result.Data.Should().NotBeNull();
        result.Data!.Images.Should().HaveCount(1);
        result.Data.Images[0].PostId.Should().Be(postPublicId);
        result.Data.Images[0].Images[0].ImageUrl.Should().Be("formatted-url");
    }

    [Fact]
    public async Task Handle_WithCursor_ShouldDecryptAndUseCursor()
    {
        var targetId = "target_public";
        var targetPrivateId = (short)10;
        var encryptedCursor = "encrypted-cursor";
        var cursor = new CursorPayload<DateTimeOffset>(DateTimeOffset.UtcNow, 5);

        _userRepository.GetPrivateIdByPublicIdAsync(targetId, Arg.Any<CancellationToken>())
            .Returns(targetPrivateId);
        _encryption.Decrypt(encryptedCursor).Returns(JsonSerializer.Serialize(cursor));
        _postReadRepository.GetPostImageAsync(targetPrivateId, 10,
                Arg.Is<CursorPayload<DateTimeOffset>?>(c => c != null && c.Id == 5), Arg.Any<CancellationToken>())
            .Returns(new CursorResult<PostImageDto, CursorPayload<DateTimeOffset>?>(new List<PostImageDto>(), false,
                null));

        await _handler.Handle(new GetListImageQuery(targetId, encryptedCursor, 10), CancellationToken.None);

        await _postReadRepository.Received(1).GetPostImageAsync(targetPrivateId, 10,
            Arg.Any<CursorPayload<DateTimeOffset>?>(), Arg.Any<CancellationToken>());
    }
}