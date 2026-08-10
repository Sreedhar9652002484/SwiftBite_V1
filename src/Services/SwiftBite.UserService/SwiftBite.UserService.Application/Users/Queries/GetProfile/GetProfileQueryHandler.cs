using MediatR;
using SwiftBite.UserService.Application.Common.Interfaces;
using SwiftBite.UserService.Application.Users.DTOs;
using SwiftBite.UserService.Domain.Entities;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Users.Queries.GetProfile;

public class GetProfileQueryHandler
    : IRequestHandler<GetProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepo;
    private readonly ICacheService _cache;

    public GetProfileQueryHandler(
        IUserRepository userRepo, ICacheService cache)
    {
        _userRepo = userRepo;
        _cache = cache;
    }

    public async Task<UserProfileDto> Handle(
        GetProfileQuery query, CancellationToken ct)
    {
        var cacheKey = $"user:profile:{query.AuthUserId}";

        // ⚡ Check cache first
        var cached = await _cache
            .GetAsync<UserProfileDto>(cacheKey, ct);
        if (cached != null) return cached;

        // 🔍 Fetch from DB
        var user = await _userRepo
            .GetByAuthUserIdAsync(query.AuthUserId, ct);

        // ✅ Auto-create user on first login!
        if (user is null)
        {
            user = User.Create(
                query.AuthUserId,
                query.FirstName ?? "User",
                query.LastName ?? "",
                query.Email ?? "",
                query.dateOfBirth);

            await _userRepo.AddAsync(user, ct);
            await _userRepo.SaveChangesAsync(ct);
        }

        var dto = new UserProfileDto
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

        // 💾 Cache for 2 minutes
        await _cache.SetAsync(
            cacheKey, dto, TimeSpan.FromMinutes(2), ct);
        return dto;
    }
}