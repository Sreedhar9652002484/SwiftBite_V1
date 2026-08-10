using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.RestaurantService.Application.MenuItems.Commands.CreateMenuItem;
using SwiftBite.RestaurantService.Application.MenuItems.Commands.DeleteMenuItem;
using SwiftBite.RestaurantService.Application.MenuItems.Queries.SearchMenuItems;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS

namespace SwiftBite.RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants/items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuItemsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Search menu items by keyword.
    /// Public endpoint - no authentication required.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ValidationException(
                "Keyword is required for search.");

        var result = await _mediator.Send(
            new SearchMenuItemsQuery(keyword), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            $"Menu items matching '{keyword}' retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Create new menu item.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ExceptionResponse), 400)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> CreateItem(
        [FromBody] CreateMenuItemRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (ownerId is null)
            throw new UnauthorizedException(
                "Owner ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new CreateMenuItemCommand(
                request.CategoryId,
                request.RestaurantId,
                ownerId,
                request.Name,
                request.Description,
                request.Price,
                request.IsVegetarian,
                request.IsVegan,
                request.IsGlutenFree,
                request.PreparationTimeMinutes,
                request.ImageUrl), ct);

        return CreatedAtAction(
            nameof(Search),
            ApiResponse<object>.SuccessResponse(
                result,
                "Menu item created successfully.",
                HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Delete menu item by ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> DeleteItem(
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
            new DeleteMenuItemCommand(id, ownerId), ct);

        return NoContent();
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

public record CreateMenuItemRequest(
    Guid CategoryId,
    Guid RestaurantId,
    string Name,
    string Description,
    decimal Price,
    bool IsVegetarian,
    bool IsVegan,
    bool IsGlutenFree,
    int PreparationTimeMinutes,
    string? ImageUrl = null);