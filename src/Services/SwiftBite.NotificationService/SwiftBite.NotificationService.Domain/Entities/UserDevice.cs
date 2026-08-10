namespace SwiftBite.NotificationService.Domain.Entities;

// ✅ Stores Firebase push token per device
public class UserDevice
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
        = string.Empty;
    public string DeviceToken { get; private set; }
        = string.Empty;
    public string DeviceType { get; private set; }
        = string.Empty; // Android / iOS / Web
    public bool IsActive { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    private UserDevice() { }

    public static UserDevice Create(
        string userId, string deviceToken,
        string deviceType)
    {
        return new UserDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeviceToken = deviceToken,
            DeviceType = deviceType,
            IsActive = true,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public void UpdateToken(string newToken)
    {
        DeviceToken = newToken;
        LastUsedAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}