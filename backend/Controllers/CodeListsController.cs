using Healthcare.Api.DTOs.CodeLists;
using Healthcare.Api.Services.CodeLists;
using Microsoft.AspNetCore.Mvc;

namespace Healthcare.Api.Controllers;

[ApiController]
[Route("api/code-lists")]
public class CodeListsController : ControllerBase
{
    private readonly ICodeListService _codeListService;

    public CodeListsController(ICodeListService codeListService)
    {
        _codeListService = codeListService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CodeListDto>>> GetAll()
    {
        var codeLists = await _codeListService.GetAllAsync();

        return Ok(codeLists);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CodeListDto>> GetById(int id)
    {
        var codeList = await _codeListService.GetByIdAsync(id);

        if (codeList is null)
            return NotFound();

        return Ok(codeList);
    }

    [HttpPost]
    public async Task<ActionResult<CodeListDto>> Create(
        CreateCodeListDto dto)
    {
        var codeList = await _codeListService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = codeList.Id },
            codeList
        );
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CodeListDto>> Update(
        int id,
        UpdateCodeListDto dto)
    {
        var codeList = await _codeListService.UpdateAsync(id, dto);

        if (codeList is null)
            return NotFound();

        return Ok(codeList);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _codeListService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}