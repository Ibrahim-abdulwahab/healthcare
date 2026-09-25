using Healthcare.Api.DTOs.Schedules;

namespace Healthcare.Api.Services.Schedules;

public interface IScheduleService
{
    Task<List<ScheduleDto>> GetAllAsync();

    Task<ScheduleDto?> GetByIdAsync(int id);

    Task<ScheduleDto> CreateAsync(CreateScheduleDto dto);

    Task<ScheduleDto?> UpdateAsync(int id, UpdateScheduleDto dto);

    Task<bool> DeleteAsync(int id);
}