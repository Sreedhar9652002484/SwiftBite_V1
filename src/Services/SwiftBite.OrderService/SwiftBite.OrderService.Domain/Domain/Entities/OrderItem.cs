namespace SwiftBite.OrderService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string? Customization { get; private set; }

    // Navigation
    public Order Order { get; private set; } = null!;

    private OrderItem() { }

    public static OrderItem Create(
        Guid menuItemId, string name,
        int quantity, decimal unitPrice,
        string? imageUrl = null,
        string? customization = null)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            MenuItemId = menuItemId,
            Name = name,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * quantity,
            ImageUrl = imageUrl,
            Customization = customization
        };
    }
}