using MediatR;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Addresses.Commands.SetDefaultAddress;

public class SetDefaultAddressCommandHandler
    : IRequestHandler<SetDefaultAddressCommand, bool>
{
    private readonly IUserRepository _userRepo;
    private readonly IAddressRepository _addressRepo;

    public SetDefaultAddressCommandHandler(
        IUserRepository userRepo,
        IAddressRepository addressRepo)
    {
        _userRepo = userRepo;
        _addressRepo = addressRepo;
    }

    public async Task<bool> Handle(
        SetDefaultAddressCommand cmd, CancellationToken ct)
    {
        var user = await _userRepo.GetByAuthUserIdAsync(cmd.AuthUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        // Unset current default
        var addresses = await _addressRepo.GetByUserIdAsync(user.Id, ct);
        foreach (var addr in addresses)
        {
            if (addr.IsDefault) addr.UnsetDefault();
            await _addressRepo.UpdateAsync(addr, ct);
        }

        // Set new default
        var newDefault = addresses.FirstOrDefault(a => a.Id == cmd.AddressId)
            ?? throw new KeyNotFoundException("Address not found.");

        if (newDefault.UserId != user.Id)
            throw new UnauthorizedAccessException(
                "Cannot update another user's address.");

        newDefault.SetAsDefault();
        await _addressRepo.UpdateAsync(newDefault, ct);
        await _addressRepo.SaveChangesAsync(ct);
        return true;
    }
}