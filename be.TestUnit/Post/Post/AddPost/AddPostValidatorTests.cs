using be.Application.Features.Post.Post.AddPost;
using be.Domain.Enums;
using FluentValidation.TestHelper;

namespace Tests.Post.Post.AddPost;

public class AddPostValidatorTests
{
    private readonly AddPostValidator _validator;

    public AddPostValidatorTests()
    {
        _validator = new AddPostValidator();
    }

    [Fact]
    public void Validator_WhenContentAndImagesAreEmpty_ShouldHaveError()
    {
        var command = new AddPostCommand { Content = "", Images = [] };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Bài viết phải có nội dung hoặc hình ảnh");
    }

    [Fact]
    public void Validator_WhenContentTooLong_ShouldHaveError()
    {
        var command = new AddPostCommand { Content = new string('a', 2001), Images = [] };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Nội dung bài viết không được vượt quá 2000 ký tự");
    }

    [Fact]
    public void Validator_WhenTooManyImages_ShouldHaveError()
    {
        var command = new AddPostCommand
        {
            Content = "Hello",
            Images = Enumerable.Range(0, 6).Select(_ => new ImageItem { Image = Guid.NewGuid() }).ToArray()
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Images)
            .WithErrorMessage("Chỉ được đăng tối đa 5 hình ảnh cho mỗi bài viết");
    }

    [Fact]
    public void Validator_WhenDuplicateImages_ShouldHaveError()
    {
        var imageId = Guid.NewGuid();
        var command = new AddPostCommand
        {
            Content = "Hello",
            Images =
            [
                new ImageItem { Image = imageId },
                new ImageItem { Image = imageId }
            ]
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Images)
            .WithErrorMessage("Không được upload url ảnh trùng lặp");
    }

    [Fact]
    public void Validator_WhenGroupHasOnlyOneImage_ShouldHaveError()
    {
        var command = new AddPostCommand
        {
            Content = "Hello",
            Images =
            [
                new ImageItem { Image = Guid.NewGuid(), GroupId = 1, Type = ImageEnum.before }
            ]
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Images)
            .WithErrorMessage("Mỗi nhóm ảnh (GroupId) phải có đúng 2 ảnh: 1 before và 1 after");
    }

    [Fact]
    public void Validator_WhenGroupMissingBefore_ShouldHaveError()
    {
        var command = new AddPostCommand
        {
            Content = "Hello",
            Images =
            [
                new ImageItem { Image = Guid.NewGuid(), GroupId = 1, Type = ImageEnum.after },
                new ImageItem { Image = Guid.NewGuid(), GroupId = 1, Type = ImageEnum.after }
            ]
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Images)
            .WithErrorMessage("Mỗi nhóm ảnh (GroupId) phải có đúng 2 ảnh: 1 before và 1 after");
    }

    [Fact]
    public void Validator_WhenNormalImageHasGroupId_ShouldHaveError()
    {
        var command = new AddPostCommand
        {
            Content = "Hello",
            Images =
            [
                new ImageItem { Image = Guid.NewGuid(), GroupId = 1, Type = ImageEnum.normal }
            ]
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Images[0]");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new AddPostCommand
        {
            Content = "Hello",
            Images =
            [
                new ImageItem { Image = Guid.NewGuid(), Type = ImageEnum.normal },
                new ImageItem { Image = Guid.NewGuid(), GroupId = 1, Type = ImageEnum.before },
                new ImageItem { Image = Guid.NewGuid(), GroupId = 1, Type = ImageEnum.after }
            ]
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}