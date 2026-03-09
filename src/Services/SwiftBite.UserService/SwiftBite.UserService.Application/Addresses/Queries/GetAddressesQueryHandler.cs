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
        var user = await _userRepo.GetByAuthUserIdAsync(query.AuthUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var addresses = await _addressRepo.GetByUserIdAsync(user.Id, ct);

        return addresses.Select(AddAddressCommandHandler.MapToDto);
    }
}