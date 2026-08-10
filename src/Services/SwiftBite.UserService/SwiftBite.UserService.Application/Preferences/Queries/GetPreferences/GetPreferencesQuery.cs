using MediatR;
using SwiftBite.UserService.Application.Preferences.DTOs;

namespace SwiftBite.UserService.Application.Preferences.Queries.GetPreferences;

public record GetPreferencesQuery(string AuthUserId)
    : IRequest<UserPreferenceDto?>;