using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.PaymentService.Application.Payments.Commands.InitiatePayment;
using SwiftBite.PaymentService.Application.Payments.Commands.RefundPayment;
using SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;
using SwiftBite.PaymentService.Application.Payments.Queries.GetCustomerPayments;
using SwiftBite.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;
using SwiftBite.PaymentService.Domain.Enums;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS

namespace SwiftBite.PaymentService.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Initiate payment - Step 1 of payment flow.
    /// </summary>
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 409)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    [ProducesResponseType(typeof(ExceptionResponse), 503)]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiatePaymentRequest request,
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (customerId is null)
            throw new UnauthorizedException(
                "Customer ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new InitiatePaymentCommand(
                request.OrderId,
                customerId,
                request.CustomerName,
                request.CustomerEmail,
                request.CustomerPhone,
                request.Amount,
                request.Method), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Payment initiated successfully. Use Razorpay details to complete payment.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Verify payment - Step 2 of payment flow.
    /// Called after customer completes Razorpay payment.
    /// </summary>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> Verify(
        [FromBody] VerifyPaymentRequest request,
        CancellationToken ct)
    {
        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new VerifyPaymentCommand(
                request.RazorpayOrderId,
                request.RazorpayPaymentId,
                request.RazorpaySignature), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Payment verified successfully! 🎉",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Refund payment.
    /// </summary>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 422)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> Refund(
        [FromBody] RefundRequest request,
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (customerId is null)
            throw new UnauthorizedException(
                "Customer ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new RefundPaymentCommand(
                request.OrderId,
                customerId,
                request.RefundAmount), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Refund initiated successfully! 💰",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Get payment by order ID.
    /// </summary>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetByOrderId(
        Guid orderId,
        CancellationToken ct)
    {
        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetPaymentByOrderIdQuery(orderId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Payment retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Get current customer's payment history.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetMyPayments(
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (customerId is null)
            throw new UnauthorizedException(
                "Customer ID not found in request.");

        var result = await _mediator.Send(
            new GetCustomerPaymentsQuery(customerId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Payment history retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Razorpay webhook - payment events from Razorpay server.
    /// No authentication required.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Webhook(
        [FromBody] object payload,
        [FromHeader(Name = "X-Razorpay-Signature")] string? signature,
        CancellationToken ct)
    {
        // ✅ In production: verify webhook signature
        // For now: log and return 200 (Razorpay expects 200!)

        // TODO: Implement webhook signature verification
        // TODO: Parse payload and process payment events

        return Ok(new { status = "received" });
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

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
    decimal RefundAmount);