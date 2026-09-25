using Healthcare.Api.Data;
using Healthcare.Api.DTOs.CodeLists;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.CodeLists;

public class CodeListService : ICodeListService
{
    private readonly HealthcareDbContext _context;

    public CodeListService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CodeListDto>> GetAllAsync()
    {
        return await _context.CodeLists
            .AsNoTracking()
            .Select(c => new CodeListDto
            {
                Id = c.Id,
                Category = c.Category,
                Code = c.Code,
                ValueEn = c.ValueEn,
                ValueAr = c.ValueAr,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CodeListDto?> GetByIdAsync(int id)
    {
        return await _context.CodeLists
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CodeListDto
            {
                Id = c.Id,
                Category = c.Category,
                Code = c.Code,
                ValueEn = c.ValueEn,
                ValueAr = c.ValueAr,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CodeListDto> CreateAsync(
        CreateCodeListDto dto)
    {
        var codeList = new CodeList
        {
            Category = dto.Category,
            Code = dto.Code,
            ValueEn = dto.ValueEn,
            ValueAr = dto.ValueAr,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.CodeLists.Add(codeList);
        await _context.SaveChangesAsync();

        return ToDto(codeList);
    }

    public async Task<CodeListDto?> UpdateAsync(
        int id,
        UpdateCodeListDto dto)
    {
        var codeList = await _context.CodeLists.FindAsync(id);

        if (codeList is null)
            return null;

        codeList.Category = dto.Category;
        codeList.Code = dto.Code;
        codeList.ValueEn = dto.ValueEn;
        codeList.ValueAr = dto.ValueAr;
        codeList.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return ToDto(codeList);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var codeList = await _context.CodeLists.FindAsync(id);

        if (codeList is null)
            return false;

        _context.CodeLists.Remove(codeList);
        await _context.SaveChangesAsync();

        return true;
    }

    private static CodeListDto ToDto(CodeList codeList)
    {
        return new CodeListDto
        {
            Id = codeList.Id,
            Category = codeList.Category,
            Code = codeList.Code,
            ValueEn = codeList.ValueEn,
            ValueAr = codeList.ValueAr,
            IsActive = codeList.IsActive,
            CreatedAt = codeList.CreatedAt
        };
    }
}