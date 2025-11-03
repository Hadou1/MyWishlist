using FluentValidation;
using Wishlist.Contracts.Lists;

namespace Wishlist.Application.Validators;

public class ListUpdateRequestValidator : AbstractValidator<ListUpdateRequest>
{
    public ListUpdateRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(120);
        RuleFor(x => x.Occasion).MaximumLength(80);
        RuleFor(x => x.Passcode).MinimumLength(4).MaximumLength(32).When(x => !string.IsNullOrWhiteSpace(x.Passcode));
    }
}
