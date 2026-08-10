using MediatR;
using SwiftBite.UserService.Application.Users.DTOs;

namespace SwiftBite.UserService.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string AuthUserId,
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth
) : IRequest<UserProfileDto>;