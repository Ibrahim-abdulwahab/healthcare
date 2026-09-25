using Healthcare.Api.DTOs.Items;
using Healthcare.Api.Services.Items;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemsController(IItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
    {
        var items = await _itemService.GetAllAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemDto>> GetById(int id)
    {
        var item = await _itemService.GetByIdAsync(id);

        if (item is null)
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(
        CreateItemDto dto)
    {
        try
        {
            var item = await _itemService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                item
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ItemDto>> Update(
        int id,
        UpdateItemDto dto)
    {
        try
        {
            var item = await _itemService.UpdateAsync(id, dto);

            if (item is null)
                return NotFound();

            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _itemService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}