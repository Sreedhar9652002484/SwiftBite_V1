using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using SwiftBite.PaymentService.API.Controllers;

namespace SwiftBite.PaymentService.Tests;

public class PaymentsControllerWebhookTests
{
    private const string WebhookSecret = "test-webhook-secret";
    private const string Payload = "{\"event\":\"payment.captured\",\"id\":\"evt_1\"}";

    private static PaymentsController BuildController(string? webhookSecret, string requestBody)
    {
        var configValues = new Dictionary<string, string?>();
        if (webhookSecret is not null)
            configValues["Razorpay:WebhookSecret"] = webhookSecret;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        var controller = new PaymentsController(Mock.Of<IMediator>(), configuration);

        var httpContext = new DefaultHttpContext
        {
            Request = { Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody)) }
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private static string ComputeSignature(string secret, string payload)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public async Task Valid_signature_returns_ok()
    {
        var controller = BuildController(WebhookSecret, Payload);
        var signature = ComputeSignature(WebhookSecret, Payload);

        var result = await controller.Webhook(signature, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Tampered_signature_returns_bad_request()
    {
        var controller = BuildController(WebhookSecret, Payload);
        var signature = ComputeSignature("wrong-secret", Payload);

        var result = await controller.Webhook(signature, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Missing_signature_header_returns_bad_request()
    {
        var controller = BuildController(WebhookSecret, Payload);

        var result = await controller.Webhook(signature: null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Missing_configured_secret_returns_bad_request()
    {
        var controller = BuildController(webhookSecret: null, Payload);
        var signature = ComputeSignature(WebhookSecret, Payload);

        var result = await controller.Webhook(signature, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
