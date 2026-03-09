using MediatR;
using SwiftBite.UserService.Application.Preferences.DTOs;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Preferences.Queries.GetPreferences;

public class GetPreferencesQueryHandler
    : IRequestHandler<GetPreferencesQuery, UserPreferenceDto?>
{
    private readonly IUserRepository _userRepo;
    private readonly IUserPreferenceRepository _prefRepo;

    public GetPreferencesQueryHandler(
        IUserRepository userRepo,
        IUserPreferenceRepository prefRepo)
    {
        _userRepo = userRepo;
        _prefRepo = prefRepo;
    }

    public async Task<UserPreferenceDto?> Handle(
        GetPreferencesQuery query, CancellationToken ct)
    {
        var user = await _userRepo.GetByAuthUserIdAsync(query.AuthUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var pref = await _prefRepo.GetByUserIdAsync(user.Id, ct);
        if (pref is null) return null;

        return new UserPreferenceDto
        {
            Id = pref.Id,
            UserId = pref.UserId,
            DietaryPreference = pref.DietaryPreference,
            EmailNotifications = pref.EmailNotifications,
            PushNotifications = pref.PushNotifications,
            SmsNotifications = pref.SmsNotifications,
            PreferredCuisines = pref.PreferredCuisines,
            AllergiesInfo = pref.AllergiesInfo
        };
    }
}