using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.Application.Preferences.DTOs;

public class UserPreferenceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DietaryPreference DietaryPreference { get; set; }
    public bool EmailNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public string PreferredCuisines { get; set; } = "[]";
    public string AllergiesInfo { get; set; } = string.Empty;
}