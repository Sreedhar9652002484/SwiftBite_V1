using MediatR;
using SwiftBite.UserService.Application.Preferences.DTOs;
using SwiftBite.UserService.Domain.Entities;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Preferences.Commands.UpdatePreferences;

public class UpdatePreferencesCommandHandler
    : IRequestHandler<UpdatePreferencesCommand, UserPreferenceDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IUserPreferenceRepository _prefRepo;

    public UpdatePreferencesCommandHandler(
        IUserRepository userRepo,
        IUserPreferenceRepository prefRepo)
    {
        _userRepo = userRepo;
        _prefRepo = prefRepo;
    }

    public async Task<UserPreferenceDto> Handle(
        UpdatePreferencesCommand cmd, CancellationToken ct)
    {
        var user = await _userRepo.GetByAuthUserIdAsync(cmd.AuthUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var pref = await _prefRepo.GetByUserIdAsync(user.Id, ct);

        if (pref is null)
        {
            pref = UserPreference.Create(user.Id);
            await _prefRepo.AddAsync(pref, ct);
        }

        pref.Update(cmd.DietaryPreference,
            cmd.EmailNotifications, cmd.PushNotifications,
            cmd.SmsNotifications, cmd.PreferredCuisines,
            cmd.AllergiesInfo);

        await _prefRepo.SaveChangesAsync(ct);

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