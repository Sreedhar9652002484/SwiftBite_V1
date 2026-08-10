using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SwiftBite.DeliveryService.API.Controllers;
using SwiftBite.DeliveryService.Application.DeliveryJobs.Commands.AcceptJob;
using SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetAvailableJobsQuery;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.UpdateAvailability;
using SwiftBite.DeliveryService.Application.DeliveryPartners.Queries.GetPartnerProfile;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.Shared.Exceptions.Exceptions;
using SwiftBite.Shared.Exceptions.Models;

namespace SwiftBite.DeliveryService.Tests;

public class DeliveryControllerTests
{
    private static DeliveryController CreateController(
        IMediator mediator, string? userId = "partner-1")
    {
        var controller = new DeliveryController(mediator);
        var httpContext = new DefaultHttpContext();

        if (userId is not null)
            httpContext.Request.Headers["X-User-Id"] = userId;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static DeliveryPartnerDto SampleProfile(string userId) => new(
        Guid.NewGuid(), userId, "Jane", "Doe", "jane@doe.com", "9999999999",
        "Bike", "KA-01-AB-1234", true, 4.8, 10, 500m, "Active", DateTime.UtcNow);

    [Fact]
    public async Task GetProfile_ReturnsOk_WhenUserIdPresent()
    {
        var mediator = new Mock<IMediator>();
        var expected = SampleProfile("partner-1");
        mediator.Setup(m => m.Send(It.IsAny<GetPartnerProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(mediator.Object);

        var result = await controller.GetProfile(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(expected, response.Data);
        mediator.Verify(m => m.Send(
            It.Is<GetPartnerProfileQuery>(q => q.UserId == "partner-1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfile_ThrowsUnauthorizedException_WhenUserIdMissing()
    {
        var controller = CreateController(Mock.Of<IMediator>(), userId: null);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => controller.GetProfile(CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAvailability_ReturnsOk_AndForwardsRequestedValue()
    {
        var mediator = new Mock<IMediator>();
        var expected = SampleProfile("partner-1") with { IsAvailable = false };
        mediator.Setup(m => m.Send(It.IsAny<UpdateAvailabilityCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(mediator.Object);

        var result = await controller.UpdateAvailability(
            new UpdateAvailabilityRequest(false), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
        mediator.Verify(m => m.Send(
            It.Is<UpdateAvailabilityCommand>(c => c.UserId == "partner-1" && c.IsAvailable == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetJobs_SendsAvailableJobsQuery_IgnoringUserId()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetAvailableJobsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DeliveryJobDto>());

        var controller = CreateController(mediator.Object);

        var result = await controller.GetJobs(CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        mediator.Verify(m => m.Send(
            It.IsAny<GetAvailableJobsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcceptJob_ThrowsUnauthorizedException_WhenUserIdMissing()
    {
        var controller = CreateController(Mock.Of<IMediator>(), userId: null);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => controller.AcceptJob(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task AcceptJob_ReturnsOk_WithAcceptedJob()
    {
        var mediator = new Mock<IMediator>();
        var jobId = Guid.NewGuid();
        var expectedJob = new DeliveryJobDto(
            jobId, Guid.NewGuid(), "ORD-1", "Alice", "8888888888",
            "Pizza Place", "123 Pickup St", "456 Drop Ave", "Bengaluru",
            50m, "Accepted", DateTime.UtcNow, DateTime.UtcNow, null, null);
        mediator.Setup(m => m.Send(It.IsAny<AcceptJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedJob);

        var controller = CreateController(mediator.Object);

        var result = await controller.AcceptJob(jobId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.Equal(expectedJob, response.Data);
        mediator.Verify(m => m.Send(
            It.Is<AcceptJobCommand>(c => c.JobId == jobId && c.UserId == "partner-1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfile_ReadsUserId_FromClaimWhenHeaderAbsent()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetPartnerProfileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleProfile("claim-user"));

        var controller = new DeliveryController(mediator.Object);
        var identity = new ClaimsIdentity(new[] { new Claim("sub", "claim-user") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        await controller.GetProfile(CancellationToken.None);

        mediator.Verify(m => m.Send(
            It.Is<GetPartnerProfileQuery>(q => q.UserId == "claim-user"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
