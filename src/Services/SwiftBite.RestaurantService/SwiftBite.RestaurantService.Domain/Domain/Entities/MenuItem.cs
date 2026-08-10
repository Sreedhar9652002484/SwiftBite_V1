using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsVegetarian { get; private set; }
    public bool IsVegan { get; private set; }
    public bool IsGlutenFree { get; private set; }
    public bool IsBestseller { get; private set; }
    public MenuItemStatus Status { get; private set; }
    public int PreparationTimeMinutes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public MenuCategory Category { get; private set; } = null!;

    private MenuItem() { }

    public static MenuItem Create(
        Guid categoryId, Guid restaurantId,
        string name, string description,
        decimal price, bool isVegetarian,
        bool isVegan, bool isGlutenFree,
        int preparationTime, string? imageUrl = null)
    {
        return new MenuItem
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            RestaurantId = restaurantId,
            Name = name,
            Description = description,
            Price = price,
            IsVegetarian = isVegetarian,
            IsVegan = isVegan,
            IsGlutenFree = isGlutenFree,
            IsBestseller = false,
            Status = MenuItemStatus.Available,
            PreparationTimeMinutes = preparationTime,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description,
        decimal price, bool isVegetarian, bool isVegan,
        bool isGlutenFree, int preparationTime, string? imageUrl)
    {
        Name = name;
        Description = description;
        Price = price;
        IsVegetarian = isVegetarian;
        IsVegan = isVegan;
        IsGlutenFree = isGlutenFree;
        PreparationTimeMinutes = preparationTime;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsBestseller() => IsBestseller = true;
    public void SetAvailable() => Status = MenuItemStatus.Available;
    public void SetUnavailable() => Status = MenuItemStatus.Unavailable;
}