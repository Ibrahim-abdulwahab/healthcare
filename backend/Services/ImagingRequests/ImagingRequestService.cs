using Healthcare.Api.Data;
using Healthcare.Api.DTOs.ImagingRequests;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.ImagingRequests;

public class ImagingRequestService : IImagingRequestService
{
    private readonly HealthcareDbContext _context;

    public ImagingRequestService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ImagingRequestDto>> GetAllAsync()
    {
        return await _context.ImagingRequests
            .AsNoTracking()
            .Select(i => new ImagingRequestDto
            {
                Id = i.Id,
                VisitId = i.VisitId,
                ServiceName = i.ServiceName,
                BodyPart = i.BodyPart,
                Priority = i.Priority,
                Status = i.Status,
                ScheduledDate = i.ScheduledDate,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ImagingRequestDto?> GetByIdAsync(int id)
    {
        return await _context.ImagingRequests
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new ImagingRequestDto
            {
                Id = i.Id,
                VisitId = i.VisitId,
                ServiceName = i.ServiceName,
                BodyPart = i.BodyPart,
                Priority = i.Priority,
                Status = i.Status,
                ScheduledDate = i.ScheduledDate,
                Notes = i.Notes,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ImagingRequestDto> CreateAsync(
        CreateImagingRequestDto dto)
    {
        await ValidateVisitAsync(dto.VisitId);

        var now = DateTime.UtcNow;

        var imagingRequest = new ImagingRequest
        {
            VisitId = dto.VisitId,
            ServiceName = dto.ServiceName,
            BodyPart = dto.BodyPart,
            Priority = dto.Priority,
            Status = dto.Status,
            ScheduledDate = dto.ScheduledDate,
            Notes = dto.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.ImagingRequests.Add(imagingRequest);
        await _context.SaveChangesAsync();

        return ToDto(imagingRequest);
    }

    public async Task<ImagingRequestDto?> UpdateAsync(
        int id,
        UpdateImagingRequestDto dto)
    {
        var imagingRequest =
            await _context.ImagingRequests.FindAsync(id);

        if (imagingRequest is null)
            return null;

        await ValidateVisitAsync(dto.VisitId);

        imagingRequest.VisitId = dto.VisitId;
        imagingRequest.ServiceName = dto.ServiceName;
        imagingRequest.BodyPart = dto.BodyPart;
        imagingRequest.Priority = dto.Priority;
        imagingRequest.Status = dto.Status;
        imagingRequest.ScheduledDate = dto.ScheduledDate;
        imagingRequest.Notes = dto.Notes;
        imagingRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(imagingRequest);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var imagingRequest =
            await _context.ImagingRequests.FindAsync(id);

        if (imagingRequest is null)
            return false;

        _context.ImagingRequests.Remove(imagingRequest);
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

    private static ImagingRequestDto ToDto(
        ImagingRequest imagingRequest)
    {
        return new ImagingRequestDto
        {
            Id = imagingRequest.Id,
            VisitId = imagingRequest.VisitId,
            ServiceName = imagingRequest.ServiceName,
            BodyPart = imagingRequest.BodyPart,
            Priority = imagingRequest.Priority,
            Status = imagingRequest.Status,
            ScheduledDate = imagingRequest.ScheduledDate,
            Notes = imagingRequest.Notes,
            CreatedAt = imagingRequest.CreatedAt,
            UpdatedAt = imagingRequest.UpdatedAt
        };
    }
}