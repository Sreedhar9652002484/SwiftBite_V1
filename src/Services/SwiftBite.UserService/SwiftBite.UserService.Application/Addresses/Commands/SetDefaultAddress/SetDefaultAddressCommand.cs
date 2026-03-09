using MediatR;

namespace SwiftBite.UserService.Application.Addresses.Commands.SetDefaultAddress;

public record SetDefaultAddressCommand(
    string AuthUserId,
    Guid AddressId
) : IRequest<bool>;