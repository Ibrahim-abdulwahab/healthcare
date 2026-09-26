using Healthcare.Api.DTOs.Schedules;

namespace Healthcare.Api.Services.Schedules;

public interface IScheduleService
{
    Task<List<ScheduleDto>> GetAllAsync();

    Task<List<ScheduleDto>> GetByDoctorIdAsync(int doctorId);

    Task<ScheduleDto?> GetByIdAsync(int id);

    Task<bool> DoctorOwnsScheduleAsync(
    int doctorId,
    int scheduleId
);

    Task<bool> DoctorOwnsLocationAsync(
    int doctorId,
    int doctorLocationId
);

    Task<ScheduleDto> CreateAsync(CreateScheduleDto dto);

    Task<ScheduleDto?> UpdateAsync(int id, UpdateScheduleDto dto);

    Task<bool> DeleteAsync(int id);
}