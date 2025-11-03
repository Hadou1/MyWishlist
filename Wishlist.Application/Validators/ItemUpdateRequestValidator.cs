using FluentValidation;
using Wishlist.Contracts.Items;

namespace Wishlist.Application.Validators;

public class ItemUpdateRequestValidator : AbstractValidator<ItemUpdateRequest>
{
    public ItemUpdateRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(160);
        RuleFor(x => x.ProductUrl).Must(ItemCreateRequestValidator.BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.ProductUrl));
        RuleFor(x => x.PriceCurrency).Length(3).When(x => !string.IsNullOrWhiteSpace(x.PriceCurrency));
        RuleFor(x => x.PriceAmount).GreaterThanOrEqualTo(0).When(x => x.PriceAmount.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
    }
}
