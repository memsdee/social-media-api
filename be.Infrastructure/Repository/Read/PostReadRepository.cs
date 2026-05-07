using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Domain.Enums;
using be.Infrastructure.Database;
using be.Infrastructure.Helper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class PostReadRepository(ReadContext dbContext) : IPostReadRepository
{
    public async Task UpdateStatusAsync(Guid publicId, StatusPostEnum status, CancellationToken ct)
    {
        await dbContext.Collection<PostDocument>()
            .UpdateOneAsync(
                x => x.IdPublic == publicId,
                Builders<PostDocument>.Update.Set(x => x.Status, status),
                cancellationToken: ct);
    }

    public async Task AddAsync(PostDocument document, CancellationToken ct)
    {
        await dbContext.Collection<PostDocument>().InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task MarkAuthorDeletedAsync(short privateAccountId, CancellationToken ct)
    {
        await dbContext.Collection<PostDocument>()
            .UpdateManyAsync(
                x => x.UserSequenceId == privateAccountId,
                Builders<PostDocument>.Update.Set(x => x.IsDeleteAccount, true),
                cancellationToken: ct);
    }

    public async Task<CursorResult<PostImageDto, CursorPayload<DateTimeOffset>?>> GetPostImageAsync(short privateUserId,
        int limit,
        CursorPayload<DateTimeOffset>? cursor, CancellationToken ct)
    {
        var filter = Builders<PostDocument>.Filter.And(
            Builders<PostDocument>.Filter.Eq(x => x.UserSequenceId, privateUserId),
            ReadCursorPagiFilterHelper.BuildCursorFilter<PostDocument, DateTimeOffset>(
                x => x.CreateAt, x => x.SequenceId, cursor?.Selector, cursor?.Id)
        );

        var rawItems = await dbContext.Collection<PostDocument>()
            .Find(filter)
            .SortByDescending(x => x.CreateAt)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .Project(x => new PostImageRawDto
            {
                SequenceId = x.SequenceId,
                PublicId = x.IdPublic,
                CreatedAt = x.CreateAt,
                Images = x.Images.Select(c => new PostImageRawItem
                {
                    Image = c.Image,
                    Type = c.ImageType,
                    GroupId = c.ImageGroupId
                }).ToArray()
            })
            .ToListAsync(ct);

        var items = rawItems.Select(x => new PostImageDto
        {
            SequenceId = x.SequenceId,
            PublicId = x.PublicId,
            CreatedAt = x.CreatedAt,
            Images = SelectPreviewImages(x.Images)
        }).ToList();

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<DateTimeOffset>(x.CreatedAt, x.SequenceId));
    }

    public async Task<CursorResult<Post1Dto, CursorPayload<short>?>> GetListPostByContentAsync(string content,
        int limit,
        CursorPayload<short>? cursor, CancellationToken ct)
    {
        var filter = Builders<PostDocument>.Filter.And(
            Builders<PostDocument>.Filter.Regex(x => x.Content, new BsonRegularExpression(content, "i")),
            ReadCursorPagiFilterHelper.BuildCursorFilter<PostDocument, short>(
                x => x.ScoreTrend, x => x.SequenceId, cursor?.Selector, cursor?.Id)
        );

        var rawItems = await dbContext.Collection<PostDocument>()
            .Find(filter)
            .SortByDescending(x => x.ScoreTrend)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .Project(x => new PostSearchRawDto
            {
                Sequence = x.SequenceId,
                Score = x.ScoreTrend,
                IdPublic = x.IdPublic,
                Content = x.Content ?? string.Empty,
                TotalComment = x.TotalCmt,
                TotalDislike = x.TotalDislike,
                TotalLike = x.TotalLike,
                CreatedAt = x.CreateAt,
                PostAuthor = new PostAuthor
                {
                    PublicUserId = x.UserIdPublic,
                    Name = x.UserName,
                    Avatar = x.UserAvatar
                },
                PostImages = x.Images
                    .Select(i => new PostImageRawItem
                    {
                        Image = i.Image,
                        Type = i.ImageType,
                        GroupId = i.ImageGroupId
                    })
                    .ToArray()
            })
            .ToListAsync(ct);

        var items = rawItems.Select(x => new Post1Dto
        {
            Sequence = x.Sequence,
            Score = x.Score,
            IdPublic = x.IdPublic,
            Content = x.Content,
            TotalComment = x.TotalComment,
            TotalDislike = x.TotalDislike,
            TotalLike = x.TotalLike,
            CreatedAt = x.CreatedAt,
            PostAuthor = x.PostAuthor,
            PostImages = GroupSearchImages(x.PostImages)
        }).ToList();

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<short>(x.Score, x.Sequence));
    }

    public async Task<CursorResult<Post1Dto, CursorPayload<DateTimeOffset>?>> GetListPostByDateAsync(
        string? targetIdPublic, HashSet<string>? followingUserIdsPublic, int limit,
        CursorPayload<DateTimeOffset>? cursor, CancellationToken ct)
    {
        var builder = Builders<PostDocument>.Filter;
        var filter = builder.Eq(x => x.Status, StatusPostEnum.active);

        if (!string.IsNullOrEmpty(targetIdPublic))
            filter &= builder.Eq(x => x.UserIdPublic, targetIdPublic);

        if (followingUserIdsPublic != null)
            filter &= builder.In(x => x.UserIdPublic, followingUserIdsPublic);

        filter &= ReadCursorPagiFilterHelper.BuildCursorFilter<PostDocument, DateTimeOffset>(
            x => x.CreateAt, x => x.SequenceId, cursor?.Selector, cursor?.Id);

        var rawItems = await dbContext.Collection<PostDocument>()
            .Find(filter)
            .SortByDescending(x => x.CreateAt)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .Project(x => new PostSearchRawDto
            {
                Sequence = x.SequenceId,
                Score = x.ScoreTrend,
                IdPublic = x.IdPublic,
                Content = x.Content ?? string.Empty,
                TotalComment = x.TotalCmt,
                TotalDislike = x.TotalDislike,
                TotalLike = x.TotalLike,
                CreatedAt = x.CreateAt,
                PostAuthor = new PostAuthor
                {
                    PublicUserId = x.UserIdPublic,
                    Name = x.UserName,
                    Avatar = x.UserAvatar
                },
                PostImages = x.Images
                    .Select(i => new PostImageRawItem
                    {
                        Image = i.Image,
                        Type = i.ImageType,
                        GroupId = i.ImageGroupId
                    })
                    .ToArray()
            })
            .ToListAsync(ct);

        var items = rawItems.Select(x => new Post1Dto
        {
            Sequence = x.Sequence,
            Score = x.Score,
            IdPublic = x.IdPublic,
            Content = x.Content,
            TotalComment = x.TotalComment,
            TotalDislike = x.TotalDislike,
            TotalLike = x.TotalLike,
            CreatedAt = x.CreatedAt,
            PostAuthor = x.PostAuthor,
            PostImages = GroupSearchImages(x.PostImages)
        }).ToList();

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<DateTimeOffset>(x.CreatedAt, x.Sequence));
    }

    public async Task<CursorResult<Post1Dto, CursorPayload<short>?>> GetListPostByScoreAsync(string? targetIdPublic,
        int limit, CursorPayload<short>? cursor, CancellationToken ct)
    {
        var builder = Builders<PostDocument>.Filter;
        var filter = builder.Eq(x => x.Status, StatusPostEnum.active);

        if (!string.IsNullOrEmpty(targetIdPublic))
            filter &= builder.Eq(x => x.UserIdPublic, targetIdPublic);

        filter &= ReadCursorPagiFilterHelper.BuildCursorFilter<PostDocument, short>(
            x => x.ScoreTrend, x => x.SequenceId, cursor?.Selector, cursor?.Id);

        var rawItems = await dbContext.Collection<PostDocument>()
            .Find(filter)
            .SortByDescending(x => x.ScoreTrend)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .Project(x => new PostSearchRawDto
            {
                Sequence = x.SequenceId,
                Score = x.ScoreTrend,
                IdPublic = x.IdPublic,
                Content = x.Content ?? string.Empty,
                TotalComment = x.TotalCmt,
                TotalDislike = x.TotalDislike,
                TotalLike = x.TotalLike,
                CreatedAt = x.CreateAt,
                PostAuthor = new PostAuthor
                {
                    PublicUserId = x.UserIdPublic,
                    Name = x.UserName,
                    Avatar = x.UserAvatar
                },
                PostImages = x.Images
                    .Select(i => new PostImageRawItem
                    {
                        Image = i.Image,
                        Type = i.ImageType,
                        GroupId = i.ImageGroupId
                    })
                    .ToArray()
            })
            .ToListAsync(ct);

        var items = rawItems.Select(x => new Post1Dto
        {
            Sequence = x.Sequence,
            Score = x.Score,
            IdPublic = x.IdPublic,
            Content = x.Content,
            TotalComment = x.TotalComment,
            TotalDislike = x.TotalDislike,
            TotalLike = x.TotalLike,
            CreatedAt = x.CreatedAt,
            PostAuthor = x.PostAuthor,
            PostImages = GroupSearchImages(x.PostImages)
        }).ToList();

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<short>(x.Score, x.Sequence));
    }

    public async Task<PostDocument?> GetByPublicIdAsync(Guid publicId, CancellationToken ct)
    {
        return await dbContext.Collection<PostDocument>()
            .Find(x => x.IdPublic == publicId)
            .FirstOrDefaultAsync(ct);
    }

    private static PostImages[] SelectPreviewImages(IEnumerable<PostImageRawItem> images)
    {
        var list = images.ToList();
        if (list.Count == 0)
            return [];

        var groupedImages = list.Where(x => x.GroupId.HasValue).ToList();
        if (groupedImages.Count > 0)
        {
            var smallestGroupId = groupedImages.Min(x => x.GroupId!.Value);
            return groupedImages
                .Where(x => x.GroupId == smallestGroupId)
                .OrderBy(x => x.Type == ImageEnum.after ? 1 : 0)
                .Take(2)
                .Select(x => new PostImages
                {
                    Image = x.Image,
                    Type = x.Type
                })
                .ToArray();
        }

        var first = list[0];
        return
        [
            new PostImages
            {
                Image = first.Image,
                Type = first.Type
            }
        ];
    }

    private static PostSearchImageDto[] GroupSearchImages(IEnumerable<PostImageRawItem> images)
    {
        return images
            .GroupBy(i => i.GroupId.HasValue ? $"g_{i.GroupId.Value}" : $"i_{i.Image}")
            .Select(g =>
            {
                var grouped = g.ToList();
                var hasPair = grouped.Any(i => i.GroupId.HasValue) && grouped.Count >= 2;
                if (!hasPair)
                {
                    var normal = grouped.First();
                    return new PostSearchImageDto
                    {
                        Type = ImageEnum.normal,
                        Image = normal.Image
                    };
                }

                var before = grouped.FirstOrDefault(i => i.Type == ImageEnum.before)?.Image;
                var after = grouped.FirstOrDefault(i => i.Type == ImageEnum.after)?.Image;

                return new PostSearchImageDto
                {
                    Type = ImageEnum.before,
                    Before = before,
                    After = after
                };
            })
            .ToArray();
    }

    private class PostImageRawDto
    {
        public short SequenceId { get; set; }
        public Guid PublicId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public PostImageRawItem[] Images { get; set; } = [];
    }

    private class PostSearchRawDto
    {
        public short Sequence { get; set; }
        public short Score { get; set; }
        public Guid IdPublic { get; set; }
        public string Content { get; set; } = string.Empty;
        public short TotalComment { get; set; }
        public short TotalDislike { get; set; }
        public short TotalLike { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public PostAuthor PostAuthor { get; set; } = null!;
        public PostImageRawItem[] PostImages { get; set; } = [];
    }

    private class PostImageRawItem
    {
        public Guid Image { get; set; }
        public ImageEnum Type { get; set; }
        public short? GroupId { get; set; }
    }
}