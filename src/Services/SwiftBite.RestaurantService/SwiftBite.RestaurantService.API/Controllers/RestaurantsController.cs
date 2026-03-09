using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.ToggleRestaurantOpen;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.UpdateRestaurant;
using SwiftBite.RestaurantService.Application.Restaurants.Queries.GetAllRestaurants;
using SwiftBite.RestaurantService.Application.Restaurants.Queries.GetRestaurantById;
using SwiftBite.RestaurantService.Application.Restaurants.Queries.GetRestaurantsByCity;
using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RestaurantsController(IMediator mediator)
        => _mediator = mediator;

    // ── GET api/restaurants ───────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAllRestaurantsQuery(), ct);
        return Ok(result);
    }

    // ── GET api/restaurants/{id} ──────────────────────────
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(
                new GetRestaurantByIdQuery(id), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET api/restaurants/city/{city} ───────────────────
    [HttpGet("city/{city}")]
    public async Task<IActionResult> GetByCity(
        string city, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetRestaurantsByCityQuery(city), ct);
        return Ok(result);
    }

    // ── POST api/restaurants ──────────────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateRestaurantRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new CreateRestaurantCommand(
                    ownerId,
                    request.Name,
                    request.Description,
                    request.PhoneNumber,
                    request.Email,
                    request.Address,
                    request.City,
                    request.PinCode,
                    request.Latitude,
                    request.Longitude,
                    request.CuisineType,
                    request.MinimumOrderAmount,
                    request.AverageDeliveryTimeMinutes), ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── PUT api/restaurants/{id} ──────────────────────────
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRestaurantRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new UpdateRestaurantCommand(
                    id, ownerId,
                    request.Name,
                    request.Description,
                    request.PhoneNumber,
                    request.Address,
                    request.City,
                    request.PinCode,
                    request.MinimumOrderAmount,
                    request.AverageDeliveryTimeMinutes,
                    request.CuisineType), ct);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // ── PUT api/restaurants/{id}/toggle ───────────────────
    [HttpPut("{id:guid}/toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleOpen(
        Guid id, CancellationToken ct)
    {
        var ownerId = GetAuthUserId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var isOpen = await _mediator.Send(
                new ToggleRestaurantOpenCommand(id, ownerId), ct);

            return Ok(new
            {
                isOpen,
                message = isOpen ? "Restaurant is now OPEN"
                                 : "Restaurant is now CLOSED"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
public record CreateRestaurantRequest(
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    string Address,
    string City,
    string PinCode,
    double Latitude,
    double Longitude,
    CuisineType CuisineType,
    decimal MinimumOrderAmount,
    int AverageDeliveryTimeMinutes);

public record UpdateRestaurantRequest(
    string Name,
    string Description,
    string PhoneNumber,
    string Address,
    string City,
    string PinCode,
    decimal MinimumOrderAmount,
    int AverageDeliveryTimeMinutes,
    CuisineType CuisineType);