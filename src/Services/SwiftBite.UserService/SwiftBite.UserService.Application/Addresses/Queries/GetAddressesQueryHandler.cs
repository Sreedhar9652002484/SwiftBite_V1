using MediatR;
using SwiftBite.UserService.Application.Addresses.Commands.AddAddress;
using SwiftBite.UserService.Application.Addresses.DTOs;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Addresses.Queries.GetAddresses;

public class GetAddressesQueryHandler
    : IRequestHandler<GetAddressesQuery, IEnumerable<AddressDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IAddressRepository _addressRepo;

    public GetAddressesQueryHandler(
        IUserRepository userRepo,
        IAddressRepository addressRepo)
    {
        _userRepo = userRepo;
        _addressRepo = addressRepo;
    }

    public async Task<IEnumerable<AddressDto>> Handle(
        GetAddressesQuery query, CancellationToken ct)
    {
        // Use the lightweight, no-tracking projection here instead of
        // GetByAuthUserIdAsync: that method eagerly Includes Addresses and
        // Preference, which meant every "get addresses" request loaded the
        // full address list (tracked) once just to resolve the user's Id,
        // then loaded it again via GetByUserIdAsync below. That duplicate
        // round-trip was pure overhead on the hot checkout-page path.
        var userId = await _userRepo.GetIdByAuthUserIdAsync(query.AuthUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var addresses = await _addressRepo.GetByUserIdAsync(userId, ct);

        return addresses.Select(AddAddressCommandHandler.MapToDto);
    }
}