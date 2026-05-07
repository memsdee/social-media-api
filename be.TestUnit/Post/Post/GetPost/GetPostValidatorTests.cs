using be.Application.Features.Post.Post.GetPost;
using FluentValidation.TestHelper;

namespace Tests.Post.Post.GetPost;

public class GetPostValidatorTests
{
    private readonly GetPostValidator _validator;

    public GetPostValidatorTests()
    {
        _validator = new GetPostValidator();
    }

    [Fact]
    public void Validator_WhenPostIdIsEmpty_ShouldHaveError()
    {
        var query = new GetPostQuery(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PostId)
            .WithErrorMessage("PostId không được để trống");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new GetPostQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}