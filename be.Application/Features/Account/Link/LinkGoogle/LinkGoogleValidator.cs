using FluentValidation;

namespace be.Application.Features.Account.Link.LinkGoogle;

public class LinkGoogleValidator : AbstractValidator<LinkGoogleCommand>
{
    public LinkGoogleValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code đang trống");
    }
}