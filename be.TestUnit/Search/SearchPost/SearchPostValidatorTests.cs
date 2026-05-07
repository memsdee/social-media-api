using be.Application.Features.Search.SearchPost;
using FluentValidation.TestHelper;

namespace Tests.Search.SearchPost;

public class SearchPostValidatorTests
{
    private readonly SearchPostValidator _validator = new();

    [Fact]
    public void Validator_WhenQueryEmpty_ShouldHaveError()
    {
        var query = new SearchPostQuery
        {
            Q = string.Empty,
            Limit = 10
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Q);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)51)]
    public void Validator_WhenLimitOutOfRange_ShouldHaveError(short limit)
    {
        var query = new SearchPostQuery
        {
            Q = "hello",
            Limit = limit
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Limit);
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new SearchPostQuery
        {
            Q = "hello",
            Limit = 10
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}