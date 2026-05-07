using FluentValidation;

namespace be.Application.Features.Notification.MarkRead;

public class MarkReadValidator : AbstractValidator<MarkReadCommand>
{
    public MarkReadValidator()
    {
        RuleFor(x => x.EncryptedIds)
            .NotEmpty()
            .WithMessage("EncryptedIds không được để trống");
    }
}