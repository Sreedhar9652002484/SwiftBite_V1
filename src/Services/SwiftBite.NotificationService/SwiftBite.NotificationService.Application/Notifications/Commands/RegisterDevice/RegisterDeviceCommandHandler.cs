using MediatR;
using SwiftBite.NotificationService.Domain.Entities;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Application.Notifications.Commands.RegisterDevice;

public class RegisterDeviceCommandHandler
    : IRequestHandler<RegisterDeviceCommand, bool>
{
    private readonly IUserDeviceRepository _deviceRepo;

    public RegisterDeviceCommandHandler(
        IUserDeviceRepository deviceRepo)
        => _deviceRepo = deviceRepo;

    public async Task<bool> Handle(
        RegisterDeviceCommand cmd, CancellationToken ct)
    {
        // ✅ Check if token already registered
        var existing = await _deviceRepo
            .GetByTokenAsync(cmd.DeviceToken, ct);

        if (existing != null)
        {
            existing.UpdateToken(cmd.DeviceToken);
            await _deviceRepo.UpdateAsync(existing, ct);
        }
        else
        {
            var device = UserDevice.Create(
                cmd.UserId, cmd.DeviceToken,
                cmd.DeviceType);
            await _deviceRepo.AddAsync(device, ct);
        }

        await _deviceRepo.SaveChangesAsync(ct);
        return true;
    }
}