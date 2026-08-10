using MediatR;

namespace SwiftBite.UserService.Application.Addresses.Commands.DeleteAddress;

public record DeleteAddressCommand(
    string AuthUserId,
    Guid AddressId
) : IRequest<bool>;