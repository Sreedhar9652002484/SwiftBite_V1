using FluentValidation.TestHelper;
using SwiftBite.RestaurantService.Application.MenuCategories.Commands.CreateMenuCategory;

namespace SwiftBite.RestaurantService.Tests.MenuCategories.Commands.CreateMenuCategory;

public class CreateMenuCategoryCommandValidatorTests
{
    private readonly CreateMenuCategoryCommandValidator _validator = new();

    private static CreateMenuCategoryCommand ValidCommand() => new(
        RestaurantId: Guid.NewGuid(),
        OwnerId: "owner-1",
        Name: "Starters",
        Description: "Appetizers and small plates",
        DisplayOrder: 1
    );

    [Fact]
    public void Should_NotHaveErrors_When_CommandIsValid()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_NameIsEmpty()
    {
        var command = ValidCommand() with { Name = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_NameExceedsMaxLength()
    {
        var command = ValidCommand() with { Name = new string('a', 51) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_DisplayOrderIsNegative()
    {
        var command = ValidCommand() with { DisplayOrder = -1 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DisplayOrder);
    }

    [Fact]
    public void Should_NotHaveError_When_DisplayOrderIsZero()
    {
        var command = ValidCommand() with { DisplayOrder = 0 };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.DisplayOrder);
    }

    [Fact]
    public void Should_NotHaveError_When_DescriptionIsNull()
    {
        var command = ValidCommand() with { Description = null };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
