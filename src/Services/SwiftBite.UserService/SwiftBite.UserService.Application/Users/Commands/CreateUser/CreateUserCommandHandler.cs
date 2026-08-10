using MediatR;
using SwiftBite.UserService.Application.Users.DTOs;
using SwiftBite.UserService.Domain.Entities;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepo;

    public CreateUserCommandHandler(IUserRepository userRepo)
        => _userRepo = userRepo;

    public async Task<UserProfileDto> Handle(
        CreateUserCommand cmd, CancellationToken ct)
    {
        // Check if already exists
        if (await _userRepo.ExistsAsync(cmd.AuthUserId, ct))
            throw new InvalidOperationException(
                $"User with AuthId {cmd.AuthUserId} already exists.");

        // Create user + default preference
        var user = User.Create(
            cmd.AuthUserId, cmd.FirstName,
            cmd.LastName, cmd.Email, cmd.DateOfBirth);

        await _userRepo.AddAsync(user, ct);
        await _userRepo.SaveChangesAsync(ct);

        return MapToDto(user);
    }

    private static UserProfileDto MapToDto(User u) => new()
    {
        Id = u.Id,
        AuthUserId = u.AuthUserId,
        FirstName = u.FirstName,
        LastName = u.LastName,
        FullName = u.FullName,
        Email = u.Email,
        PhoneNumber = u.PhoneNumber,
        ProfilePictureUrl = u.ProfilePictureUrl,
        DateOfBirth = u.DateOfBirth,
        CreatedAt = u.CreatedAt
    };
}