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
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();
        if (customerId is null) return Unauthorized();

        try
        {
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

            // 🔥 Order placed → Kafka event fired!
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── GET api/orders/{id} ───────────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(
        Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new GetOrderByIdQuery(id), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET api/orders/my ─────────────────────────────────
    // Customer sees their order history
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(
        CancellationToken ct)
    {
        var customerId = GetAuthUserId();
        if (customerId is null) return Unauthorized();

        var result = await _mediator.Send(
            new GetCustomerOrdersQuery(customerId), ct);
        return Ok(result);
    }

    // ── GET api/orders/restaurant/{restaurantId} ──────────
    // Restaurant sees incoming orders
    [HttpGet("restaurant/{restaurantId:guid}")]
    public async Task<IActionResult> GetRestaurantOrders(
        Guid restaurantId, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new GetRestaurantOrdersQuery(restaurantId), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
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
        if (requesterId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new UpdateOrderStatusCommand(
                    id, requesterId,
                    request.NewStatus), ct);

            // 🔥 Status change → Kafka event fired!
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
        if (customerId is null) return Unauthorized();

        try
        {
            await _mediator.Send(
                new CancelOrderCommand(
                    id, customerId,
                    request.Reason), ct);

            // 🔥 Order cancelled → Kafka event fired!
            return Ok(new
            {
                message = "Order cancelled successfully."
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

public record UpdateStatusRequest(OrderStatus NewStatus);
public record CancelOrderRequest(string Reason);