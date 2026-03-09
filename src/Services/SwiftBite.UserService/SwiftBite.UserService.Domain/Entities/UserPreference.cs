using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.Domain.Entities;

public class UserPreference
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public DietaryPreference DietaryPreference { get; private set; }
    public bool EmailNotifications { get; private set; }
    public bool PushNotifications { get; private set; }
    public bool SmsNotifications { get; private set; }
    public string PreferredCuisines { get; private set; } = string.Empty;
    public string AllergiesInfo { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    private UserPreference() { }

    public static UserPreference Create(Guid userId)
    {
        return new UserPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DietaryPreference = DietaryPreference.None,
            EmailNotifications = true,
            PushNotifications = true,
            SmsNotifications = false,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        DietaryPreference dietary,
        bool email, bool push, bool sms,
        string preferredCuisines, string allergiesInfo)
    {
        DietaryPreference = dietary;
        EmailNotifications = email;
        PushNotifications = push;
        SmsNotifications = sms;
        PreferredCuisines = preferredCuisines;
        AllergiesInfo = allergiesInfo;
        UpdatedAt = DateTime.UtcNow;
    }
}