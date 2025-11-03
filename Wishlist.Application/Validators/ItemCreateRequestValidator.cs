using FluentValidation;
using Wishlist.Contracts.Items;

namespace Wishlist.Application.Validators;

public class ItemCreateRequestValidator : AbstractValidator<ItemCreateRequest>
{
    public ItemCreateRequestValidator()
    {
        RuleFor(x => x.ListId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.ProductUrl).Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.ProductUrl));
        RuleFor(x => x.PriceCurrency).Length(3).When(x => !string.IsNullOrWhiteSpace(x.PriceCurrency));
        RuleFor(x => x.PriceAmount).GreaterThanOrEqualTo(0).When(x => x.PriceAmount.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }

    public static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
