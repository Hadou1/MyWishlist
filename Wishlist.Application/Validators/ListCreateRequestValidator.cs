using FluentValidation;
using Wishlist.Contracts.Lists;

namespace Wishlist.Application.Validators;

public class ListCreateRequestValidator : AbstractValidator<ListCreateRequest>
{
    public ListCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Occasion).MaximumLength(80);
    }
}
