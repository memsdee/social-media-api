using be.Application.Features.User.UpdateAvatar;
using FluentValidation.TestHelper;

namespace Tests.User.UpdateAvatar;

public class UpdateAvatarValidatorTests
{
    private readonly UpdateAvatarCommandValidator _validator = new();

    [Fact]
    public void Validator_WhenAvatarIdEmpty_ShouldHaveError()
    {
        var command = new UpdateAvatarCommand { AvatarId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AvatarId)
            .WithErrorMessage("Id avtar không được trống");
    }

    [Fact]
    public void Validator_WhenValid_ShouldNotHaveError()
    {
        var command = new UpdateAvatarCommand { AvatarId = Guid.NewGuid() };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}