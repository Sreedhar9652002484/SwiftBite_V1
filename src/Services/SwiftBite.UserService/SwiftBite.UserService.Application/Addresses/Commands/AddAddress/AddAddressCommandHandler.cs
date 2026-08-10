using MediatR;
using SwiftBite.UserService.Application.Addresses.DTOs;
using SwiftBite.UserService.Domain.Entities;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Addresses.Commands.AddAddress;

public class AddAddressCommandHandler
    : IRequestHandler<AddAddressCommand, AddressDto>
{
    private readonly IUserRepository _userRepo;
    private readonly IAddressRepository _addressRepo;

    public AddAddressCommandHandler(
        IUserRepository userRepo,
        IAddressRepository addressRepo)
    {
        _userRepo = userRepo;
        _addressRepo = addressRepo;
    }

    public async Task<AddressDto> Handle(
        AddAddressCommand cmd, CancellationToken ct)
    {
        var user = await _userRepo.GetByAuthUserIdAsync(cmd.AuthUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var address = Address.Create(
            user.Id, cmd.Label, cmd.FullAddress,
            cmd.Street, cmd.City, cmd.State,
            cmd.PinCode, cmd.Latitude, cmd.Longitude,
            cmd.Type, cmd.Landmark);

        // ✅ First address is default automatically
        var existing = await _addressRepo.GetByUserIdAsync(user.Id, ct);
        if (!existing.Any()) address.SetAsDefault();

        await _addressRepo.AddAsync(address, ct);
        await _addressRepo.SaveChangesAsync(ct);

        return MapToDto(address);
    }

    public static AddressDto MapToDto(Address a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        Label = a.Label,
        FullAddress = a.FullAddress,
        Street = a.Street,
        City = a.City,
        State = a.State,
        PinCode = a.PinCode,
        Landmark = a.Landmark,
        Latitude = a.Latitude,
        Longitude = a.Longitude,
        IsDefault = a.IsDefault,
        Type = a.Type
    };
}