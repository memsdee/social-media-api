using be.Application.Dtos.EventBus;
using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using be.Domain.Enums;
using MediatR;

namespace be.Application.Features.Post.Post.AddPost;

public class AddPostHandler(
    ICurrentUserContext currentUserContext,
    IUserRepository userRepository,
    IPostRepository postRepository,
    IOutboxRepository outboxRepository,
    IUnitOfWork unitOfWork,
    IFormat format,
    IImage image)
    : IRequestHandler<AddPostCommand, BaseResponse<PostResponse>>
{
    public async Task<BaseResponse<PostResponse>> Handle(AddPostCommand request, CancellationToken cancellationToken)
    {
        var publicUserId = currentUserContext.UserId
                           ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var user = await userRepository.GetUser2Async(publicUserId, cancellationToken)
                   ?? throw new CustomException.UnauthorizedException("Vui lòng đăng nhập lại");

        var newPost = new Domain.Entities.Post
        {
            IdPublic = Guid.NewGuid(),
            Content = request.Content,
            UserId = user.SequenceId,
            CreatAt = DateTimeOffset.UtcNow,
            Status = StatusPostEnum.active,
            PostImageNavi = request.Images.Select(x => new Domain.Entities.PostImage
            {
                Image = x.Image,
                Type = x.Type,
                GroupId = x.GroupId
            }).ToList()
        };

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await postRepository.AddAsync(newPost, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var postEvent = new PostEvent
            {
                SequenceId = newPost.Id,
                PublicId = newPost.IdPublic,
                UserSequenceId = user.SequenceId,
                UserPublicId = publicUserId,
                Content = newPost.Content,
                Status = newPost.Status,
                Images = request.Images.Select(x => new PostImageEvent
                {
                    Image = x.Image,
                    ImageType = x.Type,
                    ImageGroupId = x.GroupId
                }).ToList(),
                CreatedAt = newPost.CreatAt,
                IsDelete = false
            };

            await outboxRepository.AddAsync(OutboxTopicEnum.post, postEvent, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            if (request.Images.Length > 0)
            {
                var listImage = request.Images.Select(x => x.Image).ToList();
                await image.MoveImageAsync(listImage, cancellationToken);
            }
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new BaseResponse<PostResponse>
        {
            Data = new PostResponse
            {
                IdPublic = newPost.IdPublic,
                Content = newPost.Content,
                TotalComment = newPost.TotalComment,
                TotalLike = newPost.TotalLike,
                TotalDislike = newPost.TotalDislike,
                CreatedAt = newPost.CreatAt,
                PostAuthor = new PostAuthor
                {
                    UserId = publicUserId,
                    UserName = user.Name,
                    UserAvatar = format.FormatImageUrl(user.Avatar, publicUserId),
                    IsFollow = false
                },
                Images = newPost.PostImageNavi
                    .GroupBy(x => new { IsGrouped = x.GroupId.HasValue, Key = x.GroupId ?? x.Id })
                    .Select(x =>
                    {
                        var items = x.ToList();
                        if (!x.Key.IsGrouped)
                        {
                            var first = items.First();
                            return new PostImage
                            {
                                Image = format.FormatImageUrl(first.Image, publicUserId),
                                Type = ImageEnum.normal
                            };
                        }

                        return new PostImage
                        {
                            Image = string.Empty,
                            Type = ImageEnum.before,
                            Before = format.FormatImageUrl(items.First(i => i.Type == ImageEnum.before).Image,
                                publicUserId),
                            After = format.FormatImageUrl(items.First(i => i.Type == ImageEnum.after).Image,
                                publicUserId)
                        };
                    }).ToList()
            }
        };
    }
}