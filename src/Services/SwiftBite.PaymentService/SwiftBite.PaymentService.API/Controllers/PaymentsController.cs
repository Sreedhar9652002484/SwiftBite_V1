using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.PaymentService.Application.Payments.Commands.InitiatePayment;
using SwiftBite.PaymentService.Application.Payments.Commands.RefundPayment;
using SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;
using SwiftBite.PaymentService.Application.Payments.Queries.GetCustomerPayments;
using SwiftBite.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;
using SwiftBite.PaymentService.Domain.Enums;

namespace SwiftBite.PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
        => _mediator = mediator;

    // ── POST api/payments/initiate ────────────────────────
    // Step 1: Customer clicks Pay → creates Razorpay order
    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiatePaymentRequest request,
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();
        if (customerId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new InitiatePaymentCommand(
                    request.OrderId,
                    customerId,
                    request.CustomerName,
                    request.CustomerEmail,
                    request.CustomerPhone,
                    request.Amount,
                    request.Method), ct);

            // ✅ Returns RazorpayOrderId + KeyId to Angular
            // Angular uses this to open Razorpay checkout!
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── POST api/payments/verify ──────────────────────────
    // Step 2: After payment — Razorpay calls this to verify
    [HttpPost("verify")]
    public async Task<IActionResult> Verify(
        [FromBody] VerifyPaymentRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new VerifyPaymentCommand(
                    request.RazorpayOrderId,
                    request.RazorpayPaymentId,
                    request.RazorpaySignature), ct);

            // 🔥 Kafka event fired: swiftbite.payment.success
            return Ok(new
            {
                message = "Payment verified successfully! 🎉",
                payment = result
            });
        }
        catch (UnauthorizedAccessException)
        {
            return BadRequest(new
            {
                message = "❌ Payment signature invalid!"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST api/payments/refund ──────────────────────────
    // Customer cancels order → trigger refund
    [HttpPost("refund")]
    public async Task<IActionResult> Refund(
        [FromBody] RefundRequest request,
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();
        if (customerId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new RefundPaymentCommand(
                    request.OrderId,
                    customerId,
                    request.RefundAmount), ct);

            return Ok(new
            {
                message = "Refund initiated successfully! 💰",
                payment = result
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // ── GET api/payments/order/{orderId} ──────────────────
    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrderId(
        Guid orderId, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new GetPaymentByOrderIdQuery(orderId), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET api/payments/my ───────────────────────────────
    // Customer payment history
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayments(
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();
        if (customerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new GetCustomerPaymentsQuery(customerId), ct);
        return Ok(result);
    }

    // ── POST api/payments/webhook ─────────────────────────
    // Razorpay webhook — payment events from Razorpay server
    [HttpPost("webhook")]
    [AllowAnonymous] // ✅ Razorpay calls this — no auth token!
    public async Task<IActionResult> Webhook(
        [FromBody] object payload,
        [FromHeader(Name = "X-Razorpay-Signature")]
        string? signature,
        CancellationToken ct)
    {
        // ✅ In production: verify webhook signature here
        // For now: log and return 200 (Razorpay expects 200!)
        return Ok(new { status = "received" });
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

// ── Request Models ────────────────────────────────────────
public record InitiatePaymentRequest(
    Guid OrderId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    decimal Amount,
    PaymentMethod Method);

public record VerifyPaymentRequest(
    string RazorpayOrderId,
    string RazorpayPaymentId,
    string RazorpaySignature);

public record RefundRequest(
    Guid OrderId,
    decimal? RefundAmount = null);