using MediatR;
using SwiftBite.UserService.Application.Users.DTOs;

namespace SwiftBite.UserService.Application.Users.Queries.GetProfile;

public record GetProfileQuery(string AuthUserId) : IRequest<UserProfileDto>;