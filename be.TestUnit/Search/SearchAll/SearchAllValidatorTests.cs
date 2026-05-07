using be.Application.Features.Search.SearchAll;
using FluentValidation.TestHelper;

namespace Tests.Search.SearchAll;

public class SearchAllValidatorTests
{
    private readonly SearchAllValidator _validator = new();

    [Fact]
    public void Validator_WhenQueryEmpty_ShouldHaveError()
    {
        var query = new SearchAllQuery
        {
            Q = string.Empty,
            LimitUser = 10,
            LimitPost = 10
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Q);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)51)]
    public void Validator_WhenLimitUserOutOfRange_ShouldHaveError(short limitUser)
    {
        var query = new SearchAllQuery
        {
            Q = "hello",
            LimitUser = limitUser,
            LimitPost = 10
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.LimitUser);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)51)]
    public void Validator_WhenLimitPostOutOfRange_ShouldHaveError(short limitPost)
    {
        var query = new SearchAllQuery
        {
            Q = "hello",
            LimitUser = 10,
            LimitPost = limitPost
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.LimitPost);
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var query = new SearchAllQuery
        {
            Q = "hello",
            LimitUser = 10,
            LimitPost = 10
        };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}