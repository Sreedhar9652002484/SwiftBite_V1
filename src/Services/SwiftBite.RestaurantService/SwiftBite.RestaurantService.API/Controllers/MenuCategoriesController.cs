using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.RestaurantService.Application.MenuCategories.Commands.CreateMenuCategory;
using SwiftBite.RestaurantService.Application.MenuCategories.Commands.DeleteMenuCategory;
using SwiftBite.RestaurantService.Application.MenuCategories.Queries.GetMenuByRestaurant;

namespace SwiftBite.RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/menu")]
public class MenuCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuCategoriesController(IMediator mediator)
        => _mediator = mediator;

    // ── GET api/restaurants/{id}/menu ─────────────────────
    [HttpGet]
    public async Task<IActionResult> GetMenu(
        Guid restaurantId, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetMenuByRestaurantQuery(restaurantId), ct);
        return Ok(result);
    }

    // ── POST api/restaurants/{id}/menu ────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateCategory(
        Guid restaurantId,
        [FromBody] CreateMenuCategoryRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();
        if (ownerId is null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(
                new CreateMenuCategoryCommand(
                    restaurantId, ownerId,
                    request.Name,
                    request.Description,
                    request.DisplayOrder), ct);

            return CreatedAtAction(
                nameof(GetMenu),
                new { restaurantId }, result);
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

    // ── DELETE api/restaurants/{id}/menu/{categoryId} ─────
    [HttpDelete("{categoryId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(
        Guid restaurantId,
        Guid categoryId,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();
        if (ownerId is null) return Unauthorized();

        try
        {
            await _mediator.Send(
                new DeleteMenuCategoryCommand(
                    categoryId, ownerId), ct);
            return NoContent();
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

// ── Request Model ─────────────────────────────────────────
public record CreateMenuCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder);