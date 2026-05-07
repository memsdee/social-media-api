using FluentValidation;

namespace be.Application.Features.Search.SearchAll;

public class SearchAllValidator : AbstractValidator<SearchAllQuery>
{
    public SearchAllValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Q)
            .NotEmpty().WithMessage("Param đang trống");

        RuleFor(x => x.LimitUser).NotEmpty().WithMessage("LimitUser không được để trống")
            .InclusiveBetween((short)1, (short)50).WithMessage("LimitUser từ 1-50");

        RuleFor(x => x.LimitPost).NotEmpty().WithMessage("LimitPost không được để trống")
            .InclusiveBetween((short)1, (short)50).WithMessage("Limit Post từ 1-50");
    }
}