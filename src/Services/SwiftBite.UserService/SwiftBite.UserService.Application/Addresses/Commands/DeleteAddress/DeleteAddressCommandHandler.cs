using MediatR;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Application.Addresses.Commands.DeleteAddress;

public class DeleteAddressCommandHandler
    : IRequestHandler<DeleteAddressCommand, bool>
{
    private readonly IUserRepository _userRepo;
    private readonly IAddressRepository _addressRepo;

    public DeleteAddressCommandHandler(
        IUserRepository userRepo,
        IAddressRepository addressRepo)
    {
        _userRepo = userRepo;
        _addressRepo = addressRepo;
    }

    public async Task<bool> Handle(
        DeleteAddressCommand cmd, CancellationToken ct)
    {
        var user = await _userRepo.GetByAuthUserIdAsync(cmd.AuthUserId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        var address = await _addressRepo.GetByIdAsync(cmd.AddressId, ct)
            ?? throw new KeyNotFoundException("Address not found.");

        if (address.UserId != user.Id)
            throw new UnauthorizedAccessException(
                "Cannot delete another user's address.");

        await _addressRepo.DeleteAsync(address, ct);
        await _addressRepo.SaveChangesAsync(ct);
        return true;
    }
}