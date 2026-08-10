using FluentValidation.TestHelper;
using SwiftBite.RestaurantService.Application.MenuItems.Commands.CreateMenuItem;

namespace SwiftBite.RestaurantService.Tests.MenuItems.Commands.CreateMenuItem;

public class CreateMenuItemCommandValidatorTests
{
    private readonly CreateMenuItemCommandValidator _validator = new();

    private static CreateMenuItemCommand ValidCommand() => new(
        CategoryId: Guid.NewGuid(),
        RestaurantId: Guid.NewGuid(),
        OwnerId: "owner-1",
        Name: "Paneer Tikka",
        Description: "Grilled cottage cheese with spices",
        Price: 199.99m,
        IsVegetarian: true,
        IsVegan: false,
        IsGlutenFree: false,
        PreparationTimeMinutes: 20
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
        var command = ValidCommand() with { Name = new string('a', 101) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_DescriptionIsEmpty()
    {
        var command = ValidCommand() with { Description = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_HaveError_When_DescriptionExceedsMaxLength()
    {
        var command = ValidCommand() with { Description = new string('a', 501) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_HaveError_When_PriceIsZero()
    {
        var command = ValidCommand() with { Price = 0m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than 0.");
    }

    [Fact]
    public void Should_HaveError_When_PreparationTimeIsBelowRange()
    {
        var command = ValidCommand() with { PreparationTimeMinutes = 0 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PreparationTimeMinutes);
    }

    [Fact]
    public void Should_HaveError_When_PreparationTimeIsAboveRange()
    {
        var command = ValidCommand() with { PreparationTimeMinutes = 121 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PreparationTimeMinutes);
    }
}
