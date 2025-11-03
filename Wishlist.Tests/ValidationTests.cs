using FluentValidation.TestHelper;
using Wishlist.Application.Validators;
using Wishlist.Contracts.Items;
using Xunit;

namespace Wishlist.Tests;

public class ValidationTests
{
    [Fact]
    public void Should_Fail_ForInvalidUrl()
    {
        var validator = new ItemCreateRequestValidator();
        var result = validator.TestValidate(new ItemCreateRequest(Guid.NewGuid(), "Item", null, "ftp://invalid", null, null, "EUR", null, null, null, 1, Wishlist.Domain.Enums.ItemPriority.Medium));
        result.ShouldHaveValidationErrorFor(x => x.ProductUrl);
    }

    [Fact]
    public void Should_Fail_ForInvalidCurrency()
    {
        var validator = new ItemCreateRequestValidator();
        var result = validator.TestValidate(new ItemCreateRequest(Guid.NewGuid(), "Item", null, null, null, null, "EURO", null, null, null, 1, Wishlist.Domain.Enums.ItemPriority.Medium));
        result.ShouldHaveValidationErrorFor(x => x.PriceCurrency);
    }

    [Fact]
    public void Should_Pass_ForValidRequest()
    {
        var validator = new ItemCreateRequestValidator();
        var result = validator.TestValidate(new ItemCreateRequest(Guid.NewGuid(), "Item", null, "https://example.com", null, 10m, "EUR", null, null, null, 1, Wishlist.Domain.Enums.ItemPriority.Medium));
        result.ShouldNotHaveAnyValidationErrors();
    }
}
