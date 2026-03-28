using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.RestaurantService.Application.MenuCategories.Commands.CreateMenuCategory;
using SwiftBite.RestaurantService.Application.MenuCategories.Commands.DeleteMenuCategory;
using SwiftBite.RestaurantService.Application.MenuCategories.Queries.GetMenuByRestaurant;
using SwiftBite.Shared.Exceptions.Exceptions;  // ✅ ADD THIS
using SwiftBite.Shared.Exceptions.Models;      // ✅ ADD THIS

namespace SwiftBite.RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/menu")]
public class MenuCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuCategoriesController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Get menu categories for a restaurant.
    /// Public endpoint - no authentication required.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> GetMenu(
        Guid restaurantId,
        CancellationToken ct)
    {
        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new GetMenuByRestaurantQuery(restaurantId), ct);

        return Ok(ApiResponse<object>.SuccessResponse(
            result,
            "Menu retrieved successfully.",
            HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Create new menu category for restaurant.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), 201)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> CreateCategory(
        Guid restaurantId,
        [FromBody] CreateMenuCategoryRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (ownerId is null)
            throw new UnauthorizedException(
                "Owner ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        var result = await _mediator.Send(
            new CreateMenuCategoryCommand(
                restaurantId, ownerId,
                request.Name,
                request.Description,
                request.DisplayOrder), ct);

        return CreatedAtAction(
            nameof(GetMenu),
            new { restaurantId },
            ApiResponse<object>.SuccessResponse(
                result,
                "Menu category created successfully.",
                HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Delete menu category.
    /// </summary>
    [HttpDelete("{categoryId:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ExceptionResponse), 401)]
    [ProducesResponseType(typeof(ExceptionResponse), 403)]
    [ProducesResponseType(typeof(ExceptionResponse), 404)]
    [ProducesResponseType(typeof(ExceptionResponse), 500)]
    public async Task<IActionResult> DeleteCategory(
        Guid restaurantId,
        Guid categoryId,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();

        // ✅ CHANGE: Throw instead of return Unauthorized()
        if (ownerId is null)
            throw new UnauthorizedException(
                "Owner ID not found in request.");

        // ✅ CHANGE: NO try-catch! Middleware handles it
        await _mediator.Send(
            new DeleteMenuCategoryCommand(categoryId, ownerId), ct);

        return NoContent();
    }

    private string? GetAuthUserId()
        => Request.Headers["X-User-Id"].FirstOrDefault()
        ?? User.FindFirst("sub")?.Value;
}

public record CreateMenuCategoryRequest(
    string Name,
    string? Description = null,
    int DisplayOrder = 0);