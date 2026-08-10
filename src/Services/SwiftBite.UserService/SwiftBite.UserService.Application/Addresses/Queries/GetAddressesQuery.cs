using MediatR;
using SwiftBite.UserService.Application.Addresses.DTOs;

namespace SwiftBite.UserService.Application.Addresses.Queries.GetAddresses;

public record GetAddressesQuery(string AuthUserId)
    : IRequest<IEnumerable<AddressDto>>;