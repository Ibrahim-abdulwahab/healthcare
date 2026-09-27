using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.LabRequests;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.LabRequests;

public class LabRequestService : ILabRequestService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public LabRequestService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<LabRequestDto>> GetAllAsync()
    {
        return await _access.List<LabRequest>()
            .Select(l => new LabRequestDto
            {
                Id = l.Id,
                VisitId = l.VisitId,
                TestName = l.TestName,
                Priority = l.Priority,
                Status = l.Status,
                Notes = l.Notes,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<LabRequestDto?> GetByIdAsync(int id)
    {
        return await _access.Query<LabRequest>()
            .Where(l => l.Id == id)
            .Select(l => new LabRequestDto
            {
                Id = l.Id,
                VisitId = l.VisitId,
                TestName = l.TestName,
                Priority = l.Priority,
                Status = l.Status,
                Notes = l.Notes,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LabRequestDto> CreateAsync(CreateLabRequestDto dto)
    {
        await ValidateVisitAsync(dto.VisitId);

        var now = DateTime.UtcNow;

        var labRequest = new LabRequest
        {
            VisitId = dto.VisitId,
            TestName = dto.TestName,
            Priority = dto.Priority,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.LabRequests.Add(labRequest);
        await _context.SaveChangesAsync();

        return ToDto(labRequest);
    }

    public async Task<LabRequestDto?> UpdateAsync(
        int id,
        UpdateLabRequestDto dto)
    {
        var labRequest = await _context.LabRequests.FindAsync(id);

        if (labRequest is null)
            return null;

        await ValidateVisitAsync(dto.VisitId);

        labRequest.VisitId = dto.VisitId;
        labRequest.TestName = dto.TestName;
        labRequest.Priority = dto.Priority;
        labRequest.Status = dto.Status;
        labRequest.Notes = dto.Notes;
        labRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(labRequest);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var labRequest = await _context.LabRequests.FindAsync(id);

        if (labRequest is null)
            return false;

        _context.LabRequests.Remove(labRequest);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateVisitAsync(int visitId)
    {
        var visitExists = await _context.Visits
            .AsNoTracking()
            .AnyAsync(v => v.Id == visitId);

        if (!visitExists)
        {
            throw new ArgumentException(
                $"Visit with ID {visitId} does not exist."
            );
        }
    }

    private static LabRequestDto ToDto(LabRequest labRequest)
    {
        return new LabRequestDto
        {
            Id = labRequest.Id,
            VisitId = labRequest.VisitId,
            TestName = labRequest.TestName,
            Priority = labRequest.Priority,
            Status = labRequest.Status,
            Notes = labRequest.Notes,
            CreatedAt = labRequest.CreatedAt,
            UpdatedAt = labRequest.UpdatedAt
        };
    }
}