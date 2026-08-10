using MediatR;
using SwiftBite.UserService.Application.Common.Interfaces;
using SwiftBite.UserService.Application.Users.DTOs;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler
    : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    private readonly IUserRepository _userRepo;
    private readonly ICacheService _cache;

    public UpdateProfileCommandHandler(
        IUserRepository userRepo, ICacheService cache)
    {
        _userRepo = userRepo;
        _cache = cache;
    }

    public async Task<UserProfileDto> Handle(
        UpdateProfileCommand cmd, CancellationToken ct)
    {
        var user = await _userRepo.GetByAuthUserIdAsync(cmd.AuthUserId, ct)
            ?? throw new KeyNotFoundException(
                $"User {cmd.AuthUserId} not found.");

        user.UpdateProfile(
            cmd.FirstName, cmd.LastName,
            cmd.PhoneNumber, cmd.ProfilePictureUrl);

        await _userRepo.UpdateAsync(user, ct);
        await _userRepo.SaveChangesAsync(ct);

        // ✅ Invalidate cache after update
        await _cache.RemoveAsync($"user:profile:{cmd.AuthUserId}", ct);

        return new UserProfileDto
        {
            Id = user.Id,
            AuthUserId = user.AuthUserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            DateOfBirth = user.DateOfBirth,
            CreatedAt = user.CreatedAt
        };
    }
}