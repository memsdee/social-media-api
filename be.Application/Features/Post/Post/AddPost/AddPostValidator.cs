using be.Domain.Enums;
using FluentValidation;

namespace be.Application.Features.Post.Post.AddPost;

public class AddPostValidator : AbstractValidator<AddPostCommand>
{
    public AddPostValidator()
    {
        RuleFor(x => x.Content)
            .Must(x => x == null || x.Trim().Length <= 2000)
            .WithMessage("Nội dung bài viết không được vượt quá 2000 ký tự");

        RuleFor(x => x.Images)
            .Must(x => x.Length <= 5).WithMessage("Chỉ được đăng tối đa 5 hình ảnh cho mỗi bài viết");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || x.Images.Length > 0)
            .WithMessage("Bài viết phải có nội dung hoặc hình ảnh");

        RuleFor(x => x.Images)
            .Must(c => c.DistinctBy(i => i.Image).Count() == c.Length)
            .WithMessage("Không được upload url ảnh trùng lặp");

        RuleFor(x => x.Images)
            .Must(images =>
            {
                var grouped = images.Where(i => i.GroupId.HasValue).GroupBy(i => i.GroupId!.Value);
                return grouped.All(g =>
                    g.Count() == 2 &&
                    g.Any(i => i.Type == ImageEnum.before) &&
                    g.Any(i => i.Type == ImageEnum.after));
            })
            .WithMessage("Mỗi nhóm ảnh (GroupId) phải có đúng 2 ảnh: 1 before và 1 after");

        RuleForEach(x => x.Images)
            .Must(x => x.Type != ImageEnum.normal || !x.GroupId.HasValue)
            .WithMessage("Ảnh thường (normal) không được có GroupId");
    }
}