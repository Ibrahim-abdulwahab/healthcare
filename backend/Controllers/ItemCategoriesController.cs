using Healthcare.Api.DTOs.ItemCategories;
using Healthcare.Api.Services.ItemCategories;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemCategoriesController : ControllerBase
{
    private readonly IItemCategoryService _itemCategoryService;

    public ItemCategoriesController(
        IItemCategoryService itemCategoryService)
    {
        _itemCategoryService = itemCategoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemCategoryDto>>> GetAll()
    {
        var categories =
            await _itemCategoryService.GetAllAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemCategoryDto>> GetById(int id)
    {
        var category =
            await _itemCategoryService.GetByIdAsync(id);

        if (category is null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<ItemCategoryDto>> Create(
        CreateItemCategoryDto dto)
    {
        try
        {
            var category =
                await _itemCategoryService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                category
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ItemCategoryDto>> Update(
        int id,
        UpdateItemCategoryDto dto)
    {
        try
        {
            var category =
                await _itemCategoryService.UpdateAsync(id, dto);

            if (category is null)
                return NotFound();

            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _itemCategoryService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}