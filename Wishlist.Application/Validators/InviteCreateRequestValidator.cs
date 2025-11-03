using FluentValidation;
using Wishlist.Contracts.Invites;

namespace Wishlist.Application.Validators;

public class InviteCreateRequestValidator : AbstractValidator<InviteCreateRequest>
{
    public InviteCreateRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
