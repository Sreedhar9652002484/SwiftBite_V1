using MediatR;
using SwiftBite.UserService.Application.Preferences.DTOs;
using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.Application.Preferences.Commands.UpdatePreferences;

public record UpdatePreferencesCommand(
    string AuthUserId,
    DietaryPreference DietaryPreference,
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    string PreferredCuisines,
    string AllergiesInfo
) : IRequest<UserPreferenceDto>;