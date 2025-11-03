using FluentValidation;
using Wishlist.Contracts.Purchases;

namespace Wishlist.Application.Validators;

public class PurchaseCreateRequestValidator : AbstractValidator<PurchaseCreateRequest>
{
    public PurchaseCreateRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.PaidPriceAmount).GreaterThanOrEqualTo(0).When(x => x.PaidPriceAmount.HasValue);
        RuleFor(x => x.ReceiptUrl).Must(ItemCreateRequestValidator.BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.ReceiptUrl));
    }
}
