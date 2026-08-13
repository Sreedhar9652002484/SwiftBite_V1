using MediatR;
using SwiftBite.UserService.Domain.Interfaces;
using System.Linq;

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

        var wasDefault = address.IsDefault;

        await _addressRepo.DeleteAsync(address, ct);
        await _addressRepo.SaveChangesAsync(ct);

        // Deleting the default address must not leave the user with none —
        // promote the most recently added remaining address instead.
        if (wasDefault)
        {
            var remaining = await _addressRepo.GetByUserIdAsync(user.Id, ct);
            var next = remaining.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            if (next is not null)
            {
                next.SetAsDefault();
                await _addressRepo.UpdateAsync(next, ct);
                await _addressRepo.SaveChangesAsync(ct);
            }
        }

        return true;
    }
}