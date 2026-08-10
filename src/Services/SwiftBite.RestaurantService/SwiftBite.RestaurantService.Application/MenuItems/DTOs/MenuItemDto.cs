using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.Application.MenuItems.DTOs;

public class MenuItemDetailDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsBestseller { get; set; }
    public MenuItemStatus Status { get; set; }
    public int PreparationTimeMinutes { get; set; }
}