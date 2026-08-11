using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using OpenIddict.Abstractions;
using SwiftBite.AuthServer.Controllers;
using SwiftBite.AuthServer.Data;
using SwiftBite.AuthServer.Models;
using SwiftBite.AuthServer.Services;
using SwiftBite.Shared.Exceptions.Exceptions;

namespace SwiftBite.AuthServer.Tests;

public class PartnerApplicationsControllerTests
{
    private static AuthDbContext BuildDb()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AuthDbContext(options);
    }

    private static Mock<UserManager<ApplicationUser>> BuildUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static PartnerApplicationsController BuildController(
        AuthDbContext db, Mock<UserManager<ApplicationUser>> userManager, string userId, bool isAdmin = false,
        Mock<IRestaurantProvisioningService>? restaurantProvisioning = null)
    {
        restaurantProvisioning ??= new Mock<IRestaurantProvisioningService>();
        var controller = new PartnerApplicationsController(db, userManager.Object, restaurantProvisioning.Object);

        var claims = new List<Claim> { new(OpenIddictConstants.Claims.Subject, userId) };
        if (isAdmin)
            claims.Add(new Claim(OpenIddictConstants.Claims.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }

    private static PartnerApplicationRequest ValidRestaurantRequest() => new(
        RequestedRole: "RestaurantAdmin",
        Phone: "9876543210",
        BusinessName: "Tasty Bites",
        City: "Hyderabad");

    [Fact]
    public async Task Apply_InvalidRole_ThrowsValidationException()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "user-1");
        var request = ValidRestaurantRequest() with { RequestedRole = "SuperAdmin" };

        await Assert.ThrowsAsync<ValidationException>(() => controller.Apply(request, CancellationToken.None));
    }

    [Fact]
    public async Task Apply_RestaurantMissingBusinessName_ThrowsValidationException()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "user-1");
        var request = ValidRestaurantRequest() with { BusinessName = null };

        await Assert.ThrowsAsync<ValidationException>(() => controller.Apply(request, CancellationToken.None));
    }

    [Fact]
    public async Task Apply_RestaurantMissingCity_ThrowsValidationException()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "user-1");
        var request = ValidRestaurantRequest() with { City = null };

        await Assert.ThrowsAsync<ValidationException>(() => controller.Apply(request, CancellationToken.None));
    }

    [Fact]
    public async Task Apply_DeliveryMissingVehicleType_ThrowsValidationException()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "user-1");
        var request = new PartnerApplicationRequest(RequestedRole: "DeliveryPartner", Phone: "9876543210");

        await Assert.ThrowsAsync<ValidationException>(() => controller.Apply(request, CancellationToken.None));
    }

    [Fact]
    public async Task Apply_Valid_CreatesPendingApplication()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "user-1");

        var result = await controller.Apply(ValidRestaurantRequest(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var stored = await db.PartnerApplications.SingleAsync();
        Assert.Equal("user-1", stored.UserId);
        Assert.Equal("Pending", stored.Status);
    }

    [Fact]
    public async Task Apply_AlreadyHasPendingApplication_ThrowsValidationException()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "user-1");
        await controller.Apply(ValidRestaurantRequest(), CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(() => controller.Apply(ValidRestaurantRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task List_NonAdmin_ThrowsForbiddenException()
    {
        using var db = BuildDb();
        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.Users).Returns(new List<ApplicationUser>().AsQueryable());
        var controller = BuildController(db, userManager, "user-1", isAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() => controller.List("Pending", CancellationToken.None));
    }

    [Fact]
    public async Task Approve_NonAdmin_ThrowsForbiddenException()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "user-1", isAdmin: false);

        await Assert.ThrowsAsync<ForbiddenException>(() => controller.Approve(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Approve_UnknownApplication_ThrowsResourceNotFoundException()
    {
        using var db = BuildDb();
        var controller = BuildController(db, BuildUserManagerMock(), "admin-1", isAdmin: true);

        await Assert.ThrowsAsync<ResourceNotFoundException>(() => controller.Approve(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Approve_Valid_SetsApprovedAndAddsRole()
    {
        using var db = BuildDb();
        var applicant = new ApplicationUser { Id = "user-1", Email = "applicant@example.com" };
        var application = new PartnerApplication
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            RequestedRole = "RestaurantAdmin",
            Phone = "9876543210",
            BusinessName = "Tasty Bites"
        };
        db.PartnerApplications.Add(application);
        await db.SaveChangesAsync();

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(applicant);
        userManager.Setup(m => m.IsInRoleAsync(applicant, "RestaurantAdmin")).ReturnsAsync(false);
        userManager.Setup(m => m.AddToRoleAsync(applicant, "RestaurantAdmin")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.UpdateAsync(applicant)).ReturnsAsync(IdentityResult.Success);

        var restaurantId = Guid.NewGuid();
        var restaurantProvisioning = new Mock<IRestaurantProvisioningService>();
        restaurantProvisioning
            .Setup(s => s.CreateRestaurantAsync(applicant, application, It.IsAny<CancellationToken>()))
            .ReturnsAsync(restaurantId);

        var controller = BuildController(db, userManager, "admin-1", isAdmin: true, restaurantProvisioning);

        var result = await controller.Approve(application.Id, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        userManager.Verify(m => m.AddToRoleAsync(applicant, "RestaurantAdmin"), Times.Once);
        Assert.Equal(restaurantId, applicant.RestaurantId);
        var updated = await db.PartnerApplications.SingleAsync();
        Assert.Equal("Approved", updated.Status);
        Assert.Equal("admin-1", updated.ReviewedByUserId);
    }

    [Fact]
    public async Task Approve_DeliveryPartner_DoesNotProvisionRestaurant()
    {
        using var db = BuildDb();
        var applicant = new ApplicationUser { Id = "user-1", Email = "rider@example.com" };
        var application = new PartnerApplication
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            RequestedRole = "DeliveryPartner",
            Phone = "9876543210",
            VehicleType = "Bike"
        };
        db.PartnerApplications.Add(application);
        await db.SaveChangesAsync();

        var userManager = BuildUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(applicant);
        userManager.Setup(m => m.IsInRoleAsync(applicant, "DeliveryPartner")).ReturnsAsync(false);
        userManager.Setup(m => m.AddToRoleAsync(applicant, "DeliveryPartner")).ReturnsAsync(IdentityResult.Success);

        var restaurantProvisioning = new Mock<IRestaurantProvisioningService>();
        var controller = BuildController(db, userManager, "admin-1", isAdmin: true, restaurantProvisioning);

        var result = await controller.Approve(application.Id, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        restaurantProvisioning.Verify(
            s => s.CreateRestaurantAsync(It.IsAny<ApplicationUser>(), It.IsAny<PartnerApplication>(), It.IsAny<CancellationToken>()),
            Times.Never);
        Assert.Null(applicant.RestaurantId);
    }

    [Fact]
    public async Task Reject_Valid_SetsRejectedWithNote()
    {
        using var db = BuildDb();
        var application = new PartnerApplication
        {
            Id = Guid.NewGuid(),
            UserId = "user-1",
            RequestedRole = "DeliveryPartner",
            Phone = "9876543210",
            VehicleType = "Bike"
        };
        db.PartnerApplications.Add(application);
        await db.SaveChangesAsync();

        var controller = BuildController(db, BuildUserManagerMock(), "admin-1", isAdmin: true);

        var result = await controller.Reject(application.Id, new RejectApplicationRequest("Incomplete documents"), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        var updated = await db.PartnerApplications.SingleAsync();
        Assert.Equal("Rejected", updated.Status);
        Assert.Equal("Incomplete documents", updated.ReviewNote);
    }
}
