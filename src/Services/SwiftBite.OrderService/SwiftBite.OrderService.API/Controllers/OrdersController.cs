using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.OrderService.Application.Orders.Commands.CancelOrder;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.Commands.UpdateOrderStatus;
using SwiftBite.OrderService.Application.Orders.Queries.GetCustomerOrders;
using SwiftBite.OrderService.Application.Orders.Queries.GetOrderById;
using SwiftBite.OrderService.Application.Orders.Queries.GetRestaurantOrders;
using SwiftBite.OrderService.Domain.Enums;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;

namespace SwiftBite.OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
        => _mediator = mediator;

    // ── POST api/orders ───────────────────────────────────
    // Customer places an order
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();
        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (customerId is null)
            throw new UnauthorizedException(
                "Customer ID not found in request.");
       
        var result = await _mediator.Send(
                new PlaceOrderCommand(
                    customerId,
                    request.CustomerName,
                    request.CustomerPhone,
                    request.RestaurantId,
                    request.RestaurantName,
                    request.DeliveryAddress,
                    request.DeliveryCity,
                    request.DeliveryPinCode,
                    request.DeliveryLatitude,
                    request.DeliveryLongitude,
                    request.PaymentMethod,
                    request.SpecialInstructions,
                    request.Items), ct);

            return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Id },
            ApiResponse<object>.SuccessResponse(
                result,
                "Order placed successfully.",
                HttpContext.TraceIdentifier));
      
    }

    // ── GET api/orders/{id} ───────────────────────────────
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetOrderById(
        Guid id, CancellationToken ct)
    {
        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetOrderByIdQuery(id), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Order retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── GET api/orders/my ─────────────────────────────────
    // Customer sees their order history
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (customerId is null)
            throw new UnauthorizedException(
                "Customer ID not found in request.");

        var result = await _mediator.Send(
            new GetCustomerOrdersQuery(customerId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Orders retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── GET api/orders/restaurant/{restaurantId} ──────────
    // Restaurant sees incoming orders
    [HttpGet("restaurant/{restaurantId:guid}")]
    public async Task<IActionResult> GetRestaurantOrders(
        Guid restaurantId, CancellationToken ct)
    {
        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetRestaurantOrdersQuery(restaurantId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Restaurant orders retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── PUT api/orders/{id}/status ────────────────────────
    // Restaurant/Delivery updates order status
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken ct)
    {
        var requesterId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (requesterId is null)
            throw new UnauthorizedException(
                "Requester ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new UpdateOrderStatusCommand(
                id, requesterId,
                request.NewStatus, request.RowVersion), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Order status updated successfully.",
            HttpContext.TraceIdentifier));
    }

    // ── DELETE api/orders/{id} ────────────────────────────
    // Customer cancels order
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (customerId is null)
            throw new UnauthorizedException(
                "Customer ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new CancelOrderCommand(
                id, customerId, request.Reason), ct);

        return Ok(ApiResponse.SuccessResponse(
            "Order cancelled successfully.",
            HttpContext.TraceIdentifier));
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

// ── Request Models ────────────────────────────────────────
public record PlaceOrderRequest(
    string CustomerName,
    string CustomerPhone,
    Guid RestaurantId,
    string RestaurantName,
    string DeliveryAddress,
    string DeliveryCity,
    string DeliveryPinCode,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string PaymentMethod,
    string? SpecialInstructions,
    List<OrderItemRequest> Items);

public record UpdateStatusRequest(OrderStatus NewStatus, string RowVersion);
public record CancelOrderRequest(string Reason);