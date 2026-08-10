using MediatR;
using SwiftBite.UserService.Application.Addresses.DTOs;
using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.Application.Addresses.Commands.AddAddress;

public record AddAddressCommand(
    string AuthUserId,
    string Label,
    string FullAddress,
    string Street,
    string City,
    string State,
    string PinCode,
    double Latitude,
    double Longitude,
    AddressType Type,
    string? Landmark = null
) : IRequest<AddressDto>;