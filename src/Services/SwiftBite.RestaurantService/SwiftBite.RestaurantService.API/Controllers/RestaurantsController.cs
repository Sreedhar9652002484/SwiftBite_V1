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
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS

namespace SwiftBite.RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RestaurantsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Get all restaurants.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetAllRestaurantsQuery(), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "All restaurants retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Get restaurant by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken ct)
    {
        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetRestaurantByIdQuery(id), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Restaurant retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Get restaurants by city.
    /// </summary>
    [HttpGet("city/{city}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetByCity(
        string city,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ValidationException(
                "City name cannot be empty.");

        var result = await _mediator.Send(
            new GetRestaurantsByCityQuery(city), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            $"Restaurants in {city} retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Create new restaurant.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 409)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRestaurantRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (ownerId is null)
            throw new UnauthorizedException(
                "Owner ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
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
            new { id = result.Id },
            ApiResponse<object>.SuccessResponse(
                result,
                "Restaurant created successfully.",
                HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Update restaurant details.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRestaurantRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (ownerId is null)
            throw new UnauthorizedException(
                "Owner ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
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

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Restaurant updated successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Toggle restaurant open/closed status.
    /// </summary>
    [HttpPut("{id:guid}/toggle")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> ToggleOpen(
        Guid id,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (ownerId is null)
            throw new UnauthorizedException(
                "Owner ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new ToggleRestaurantOpenCommand(id, ownerId), ct);

        return Ok(ApiResponse.SuccessResponse(
            "Restaurant status toggled successfully.",
            HttpContext.TraceIdentifier));
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

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