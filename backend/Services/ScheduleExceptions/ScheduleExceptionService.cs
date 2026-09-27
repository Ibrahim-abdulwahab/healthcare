using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.ScheduleExceptions;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.ScheduleExceptions;

public class ScheduleExceptionService : IScheduleExceptionService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public ScheduleExceptionService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<List<ScheduleExceptionDto>> GetAllAsync()
    {
        return await _access.List<ScheduleException>()
            .Select(se => new ScheduleExceptionDto
            {
                Id = se.Id,
                DoctorLocationId = se.DoctorLocationId,
                Date = se.Date,
                StartTime = se.StartTime,
                EndTime = se.EndTime,
                IsAvailable = se.IsAvailable,
                Reason = se.Reason,
                CreatedAt = se.CreatedAt,
                UpdatedAt = se.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ScheduleExceptionDto?> GetByIdAsync(int id)
    {
        return await _access.Query<ScheduleException>()
            .Where(se => se.Id == id)
            .Select(se => new ScheduleExceptionDto
            {
                Id = se.Id,
                DoctorLocationId = se.DoctorLocationId,
                Date = se.Date,
                StartTime = se.StartTime,
                EndTime = se.EndTime,
                IsAvailable = se.IsAvailable,
                Reason = se.Reason,
                CreatedAt = se.CreatedAt,
                UpdatedAt = se.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DoctorOwnsLocationAsync(
    int doctorId,
    int doctorLocationId)
    {
        return await _context.DoctorLocations
            .AsNoTracking()
            .AnyAsync(dl =>
                dl.Id == doctorLocationId &&
                dl.DoctorId == doctorId
            );
    }

    public async Task<bool> DoctorOwnsExceptionAsync(
        int doctorId,
        int scheduleExceptionId)
    {
        return await _context.ScheduleExceptions
            .AsNoTracking()
            .AnyAsync(se =>
                se.Id == scheduleExceptionId &&
                se.DoctorLocation.DoctorId == doctorId
            );
    }

    public async Task<ScheduleExceptionDto> CreateAsync(
    CreateScheduleExceptionDto dto)
    {
        var doctorLocationExists = await _context.DoctorLocations
            .AnyAsync(dl => dl.Id == dto.DoctorLocationId);

        if (!doctorLocationExists)
        {
            throw new ArgumentException(
                $"DoctorLocation with ID {dto.DoctorLocationId} does not exist."
            );
        }

        if (dto.EndTime <= dto.StartTime)
        {
            throw new ArgumentException(
                "EndTime must be later than StartTime."
            );
        }

        var now = DateTime.UtcNow;

        var scheduleException = new ScheduleException
        {
            DoctorLocationId = dto.DoctorLocationId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            IsAvailable = dto.IsAvailable,
            Reason = dto.Reason,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.ScheduleExceptions.Add(scheduleException);

        await _context.SaveChangesAsync();

        return new ScheduleExceptionDto
        {
            Id = scheduleException.Id,
            DoctorLocationId = scheduleException.DoctorLocationId,
            Date = scheduleException.Date,
            StartTime = scheduleException.StartTime,
            EndTime = scheduleException.EndTime,
            IsAvailable = scheduleException.IsAvailable,
            Reason = scheduleException.Reason,
            CreatedAt = scheduleException.CreatedAt,
            UpdatedAt = scheduleException.UpdatedAt
        };
    }

    public async Task<ScheduleExceptionDto?> UpdateAsync(
    int id,
    UpdateScheduleExceptionDto dto)
    {
        var scheduleException = await _context.ScheduleExceptions
            .FirstOrDefaultAsync(se => se.Id == id);

        if (scheduleException is null)
        {
            return null;
        }

        var doctorLocationExists = await _context.DoctorLocations
            .AnyAsync(dl => dl.Id == dto.DoctorLocationId);

        if (!doctorLocationExists)
        {
            throw new ArgumentException(
                $"DoctorLocation with ID {dto.DoctorLocationId} does not exist."
            );
        }

        if (dto.EndTime <= dto.StartTime)
        {
            throw new ArgumentException(
                "EndTime must be later than StartTime."
            );
        }

        scheduleException.DoctorLocationId = dto.DoctorLocationId;
        scheduleException.Date = dto.Date;
        scheduleException.StartTime = dto.StartTime;
        scheduleException.EndTime = dto.EndTime;
        scheduleException.IsAvailable = dto.IsAvailable;
        scheduleException.Reason = dto.Reason;
        scheduleException.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ScheduleExceptionDto
        {
            Id = scheduleException.Id,
            DoctorLocationId = scheduleException.DoctorLocationId,
            Date = scheduleException.Date,
            StartTime = scheduleException.StartTime,
            EndTime = scheduleException.EndTime,
            IsAvailable = scheduleException.IsAvailable,
            Reason = scheduleException.Reason,
            CreatedAt = scheduleException.CreatedAt,
            UpdatedAt = scheduleException.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var scheduleException = await _context.ScheduleExceptions
            .FirstOrDefaultAsync(se => se.Id == id);

        if (scheduleException is null)
        {
            return false;
        }

        _context.ScheduleExceptions.Remove(scheduleException);

        await _context.SaveChangesAsync();

        return true;
    }
}