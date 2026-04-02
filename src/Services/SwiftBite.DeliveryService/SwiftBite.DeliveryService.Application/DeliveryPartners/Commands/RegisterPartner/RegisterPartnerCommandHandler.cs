// ── Handler ──────────────────────────────────────────────────
using MediatR;
using SwiftBite.DeliveryService.Application;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.RegisterPartner;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Entities;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;

public class RegisterPartnerCommandHandler
    : IRequestHandler<RegisterPartnerCommand, DeliveryPartnerDto>
{
    private readonly IDeliveryPartnerRepository _repo;

    public RegisterPartnerCommandHandler(IDeliveryPartnerRepository repo)
        => _repo = repo;

    public async Task<DeliveryPartnerDto> Handle(
        RegisterPartnerCommand cmd,
        CancellationToken ct)
    {
        if (await _repo.ExistsByUserIdAsync(cmd.UserId, ct))
            throw new InvalidOperationException("Partner already registered.");

        var partner = DeliveryPartner.Create(
            cmd.UserId, cmd.FirstName, cmd.LastName,
            cmd.Email, cmd.PhoneNumber,
            cmd.VehicleType, cmd.VehicleNumber);

        await _repo.AddAsync(partner, ct);
        await _repo.SaveChangesAsync(ct);

        return partner.ToDto();


    }

}