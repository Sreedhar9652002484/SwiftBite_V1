namespace SwiftBite.RestaurantService.Domain.Entities;

public class RestaurantHours
{
    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan OpenTime { get; private set; }
    public TimeSpan CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    // Navigation
    public Restaurant Restaurant { get; private set; } = null!;

    private RestaurantHours() { }

    public static RestaurantHours Create(
        Guid restaurantId, DayOfWeek day,
        TimeSpan openTime, TimeSpan closeTime,
        bool isClosed = false)
    {
        return new RestaurantHours
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantId,
            DayOfWeek = day,
            OpenTime = openTime,
            CloseTime = closeTime,
            IsClosed = isClosed
        };
    }

    public void Update(TimeSpan openTime,
        TimeSpan closeTime, bool isClosed)
    {
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }
}