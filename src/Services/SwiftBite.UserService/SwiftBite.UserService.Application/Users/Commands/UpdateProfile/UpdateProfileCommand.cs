using MediatR;
using SwiftBite.UserService.Application.Users.DTOs;

namespace SwiftBite.UserService.Application.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string AuthUserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfilePictureUrl
) : IRequest<UserProfileDto>;