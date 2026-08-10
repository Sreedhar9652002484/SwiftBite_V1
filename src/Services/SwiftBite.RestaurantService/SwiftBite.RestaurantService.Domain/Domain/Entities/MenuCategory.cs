namespace SwiftBite.RestaurantService.Domain.Entities;

public class MenuCategory
{
    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public Restaurant Restaurant { get; private set; } = null!;
    public ICollection<MenuItem> MenuItems { get; private set; }
        = new List<MenuItem>();

    private MenuCategory() { }

    public static MenuCategory Create(
        Guid restaurantId, string name,
        string? description, int displayOrder)
    {
        return new MenuCategory
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, int displayOrder)
    {
        Name = name;
        Description = description;
        DisplayOrder = displayOrder;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}