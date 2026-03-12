using MediatR;

namespace SwiftBite.NotificationService.Application.Notifications.Commands.RegisterDevice;

public record RegisterDeviceCommand(
    string UserId,
    string DeviceToken,
    string DeviceType  // Android / iOS / Web
) : IRequest<bool>;