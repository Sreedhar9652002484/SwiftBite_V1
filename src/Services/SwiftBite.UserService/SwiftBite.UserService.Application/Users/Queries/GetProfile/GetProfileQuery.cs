using MediatR;
using SwiftBite.UserService.Application.Users.DTOs;

namespace SwiftBite.UserService.Application.Users.Queries.GetProfile;

public record GetProfileQuery(
    string AuthUserId,
    string? FirstName,
    string? LastName,
    string? Email, DateTime dateOfBirth) : IRequest<UserProfileDto>;