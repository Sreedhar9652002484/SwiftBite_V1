using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftBite.RestaurantService.Application.MenuItems.Commands.CreateMenuItem;
using SwiftBite.RestaurantService.Application.MenuItems.Commands.DeleteMenuItem;
using SwiftBite.RestaurantService.Application.MenuItems.Queries.SearchMenuItems;

namespace SwiftBite.RestaurantService.API.Controllers;

[ApiController]
[Route("api/restaurants/items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuItemsController(IMediator mediator)
        => _mediator = mediator;

    // ── GET api/restaurants/items/search?keyword=biryani ──
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string keyword,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest(new
            {
                message = "Keyword is required."
            });

        var result = await _mediator.Send(
            new SearchMenuItemsQuery(keyword), ct);
        return Ok(result);
    }

    // ── POST api/restaurants/items ────────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateItem(
        [FromBody] CreateMenuItemRequest request,
        CancellationToken ct)
    {
        var ownerId = GetAuthUserId();
        if (ownerId is null) return Unauthorized();

        try
        {
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
                nameof(Search), result);
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

    // ── DELETE api/restaurants/items/{id} ─────────────────
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteItem(
        Guid id, CancellationToken ct)
    {
        var ownerId = GetAuthUserId();
        if (ownerId is null) return Unauthorized();

        try
        {
            await _mediator.Send(
                new DeleteMenuItemCommand(id, ownerId), ct);
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