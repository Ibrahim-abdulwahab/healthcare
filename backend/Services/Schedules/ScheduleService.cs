using Healthcare.Api.Data;
using Healthcare.Api.DTOs.Schedules;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;


namespace Healthcare.Api.Services.Schedules;

public class ScheduleService : IScheduleService
{
    private readonly HealthcareDbContext _context;

    public ScheduleService(HealthcareDbContext context)
    {
        _context = context;
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

    public async Task<bool> DoctorOwnsScheduleAsync(
    int doctorId,
    int scheduleId)
    {
        return await _context.Schedules
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id == scheduleId &&
                s.DoctorLocation.DoctorId == doctorId
            );
    }

    public async Task<List<ScheduleDto>> GetAllAsync()
    {
        return await _context.Schedules
            .AsNoTracking()
            .Select(s => new ScheduleDto
            {
                Id = s.Id,
                DoctorLocationId = s.DoctorLocationId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDuration = s.SlotDuration,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<ScheduleDto>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Where(s => s.DoctorLocation.DoctorId == doctorId)
            .Select(s => new ScheduleDto
            {
                Id = s.Id,
                DoctorLocationId = s.DoctorLocationId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDuration = s.SlotDuration,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ScheduleDto?> GetByIdAsync(int id)
    {
        return await _context.Schedules
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ScheduleDto
            {
                Id = s.Id,
                DoctorLocationId = s.DoctorLocationId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SlotDuration = s.SlotDuration,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ScheduleDto> CreateAsync(CreateScheduleDto dto)
    {
        var doctorLocationExists = await _context.DoctorLocations
            .AnyAsync(dl => dl.Id == dto.DoctorLocationId);

        if (!doctorLocationExists)
        {
            throw new ArgumentException(
                $"DoctorLocation with ID {dto.DoctorLocationId} does not exist."
            );
        }

        if (dto.DayOfWeek < 0 || dto.DayOfWeek > 6)
        {
            throw new ArgumentException(
                "DayOfWeek must be between 0 and 6."
            );
        }

        if (dto.EndTime <= dto.StartTime)
        {
            throw new ArgumentException(
                "EndTime must be later than StartTime."
            );
        }

        if (dto.SlotDuration <= 0)
        {
            throw new ArgumentException(
                "SlotDuration must be greater than 0."
            );
        }

        var now = DateTime.UtcNow;

        var schedule = new Schedule
        {
            DoctorLocationId = dto.DoctorLocationId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            SlotDuration = dto.SlotDuration,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Schedules.Add(schedule);

        await _context.SaveChangesAsync();

        return new ScheduleDto
        {
            Id = schedule.Id,
            DoctorLocationId = schedule.DoctorLocationId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            SlotDuration = schedule.SlotDuration,
            IsActive = schedule.IsActive,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }

    public async Task<ScheduleDto?> UpdateAsync(
    int id,
    UpdateScheduleDto dto)
    {
        var schedule = await _context.Schedules
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule is null)
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

        if (dto.DayOfWeek < 0 || dto.DayOfWeek > 6)
        {
            throw new ArgumentException(
                "DayOfWeek must be between 0 and 6."
            );
        }

        if (dto.EndTime <= dto.StartTime)
        {
            throw new ArgumentException(
                "EndTime must be later than StartTime."
            );
        }

        if (dto.SlotDuration <= 0)
        {
            throw new ArgumentException(
                "SlotDuration must be greater than 0."
            );
        }

        schedule.DoctorLocationId = dto.DoctorLocationId;
        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.StartTime = dto.StartTime;
        schedule.EndTime = dto.EndTime;
        schedule.SlotDuration = dto.SlotDuration;
        schedule.IsActive = dto.IsActive;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ScheduleDto
        {
            Id = schedule.Id,
            DoctorLocationId = schedule.DoctorLocationId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            SlotDuration = schedule.SlotDuration,
            IsActive = schedule.IsActive,
            CreatedAt = schedule.CreatedAt,
            UpdatedAt = schedule.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var schedule = await _context.Schedules
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule is null)
        {
            return false;
        }

        _context.Schedules.Remove(schedule);

        await _context.SaveChangesAsync();

        return true;
    }
}