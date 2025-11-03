using FluentValidation;
using Wishlist.Contracts.Reservations;

namespace Wishlist.Application.Validators;

public class ReservationCreateRequestValidator : AbstractValidator<ReservationCreateRequest>
{
    public ReservationCreateRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
