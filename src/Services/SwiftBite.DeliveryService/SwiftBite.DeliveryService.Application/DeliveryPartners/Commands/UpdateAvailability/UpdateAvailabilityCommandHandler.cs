using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.UpdateAvailability;

public class UpdateAvailabilityCommandHandler
    : IRequestHandler<UpdateAvailabilityCommand, DeliveryPartnerDto>
{
    private readonly IDeliveryPartnerRepository _repo;

    public UpdateAvailabilityCommandHandler(IDeliveryPartnerRepository repo)
        => _repo = repo;

    public async Task<DeliveryPartnerDto> Handle(
        UpdateAvailabilityCommand cmd, CancellationToken ct)
    {
        var partner = await _repo.GetByUserIdAsync(cmd.UserId, ct)
            ?? throw new KeyNotFoundException("Partner not found.");

        partner.SetAvailability(cmd.IsAvailable);
        await _repo.SaveChangesAsync(ct);

        return partner.ToDto();
    }
}