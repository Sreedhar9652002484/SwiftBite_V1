using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SwiftBite.NotificationService.API.Controllers;
using SwiftBite.NotificationService.Application.Notifications.Commands.MarkAllRead;
using SwiftBite.NotificationService.Application.Notifications.Commands.RegisterDevice;
using SwiftBite.NotificationService.Application.Notifications.Commands.SendNotification;
using SwiftBite.NotificationService.Application.Notifications.DTOs;
using SwiftBite.NotificationService.Application.Notifications.Queries.GetNotifications;
using SwiftBite.NotificationService.Application.Notifications.Queries.GetUnreadCount;
using SwiftBite.NotificationService.Domain.Enums;
using SwiftBite.Shared.Exceptions.Exceptions;

namespace SwiftBite.NotificationService.Tests;

public class NotificationsControllerTests
{
    private static NotificationsController BuildController(Mock<IMediator> mediator, string? userIdHeader = "user-1")
    {
        var controller = new NotificationsController(mediator.Object);
        var httpContext = new DefaultHttpContext();
        if (userIdHeader is not null)
            httpContext.Request.Headers["X-User-Id"] = userIdHeader;

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    [Fact]
    public async Task GetAll_returns_ok_when_authenticated()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetNotificationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationListDto());
        var controller = BuildController(mediator);

        var result = await controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAll_throws_when_user_id_missing()
    {
        var mediator = new Mock<IMediator>();
        var controller = BuildController(mediator, userIdHeader: null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => controller.GetAll());
    }

    [Fact]
    public async Task GetUnreadCount_returns_ok_when_authenticated()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetUnreadCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        var controller = BuildController(mediator);

        var result = await controller.GetUnreadCount();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task MarkAllRead_throws_when_user_id_missing()
    {
        var mediator = new Mock<IMediator>();
        var controller = BuildController(mediator, userIdHeader: null);

        await Assert.ThrowsAsync<UnauthorizedException>(() => controller.MarkAllRead());
    }

    [Fact]
    public async Task RegisterDevice_throws_when_device_token_empty()
    {
        var mediator = new Mock<IMediator>();
        var controller = BuildController(mediator);
        var request = new RegisterDeviceRequest(DeviceToken: "", DeviceType: "android");

        await Assert.ThrowsAsync<ValidationException>(() => controller.RegisterDevice(request));
    }

    [Fact]
    public async Task RegisterDevice_returns_ok_for_valid_request()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<RegisterDeviceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = BuildController(mediator);
        var request = new RegisterDeviceRequest(DeviceToken: "token-123", DeviceType: "android");

        var result = await controller.RegisterDevice(request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Send_throws_when_title_empty()
    {
        var mediator = new Mock<IMediator>();
        var controller = BuildController(mediator);
        var request = new SendNotificationRequest(UserId: "user-1", Title: "", Message: "hello", Type: NotificationType.OrderPlaced);

        await Assert.ThrowsAsync<ValidationException>(() => controller.Send(request));
    }

    [Fact]
    public async Task Send_returns_ok_for_valid_request()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = BuildController(mediator);
        var request = new SendNotificationRequest(UserId: "user-1", Title: "Order update", Message: "Your order shipped", Type: NotificationType.OrderPlaced);

        var result = await controller.Send(request);

        Assert.IsType<OkObjectResult>(result);
    }
}
