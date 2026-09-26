using Healthcare.Api.DTOs.ScheduleExceptions;

namespace Healthcare.Api.Services.ScheduleExceptions;

public interface IScheduleExceptionService
{
    Task<List<ScheduleExceptionDto>> GetAllAsync();

    Task<ScheduleExceptionDto?> GetByIdAsync(int id);

    Task<bool> DoctorOwnsLocationAsync(
    int doctorId,
    int doctorLocationId
    );

    Task<bool> DoctorOwnsExceptionAsync(
        int doctorId,
        int scheduleExceptionId
    );

    Task<ScheduleExceptionDto> CreateAsync(
        CreateScheduleExceptionDto dto);

    Task<ScheduleExceptionDto?> UpdateAsync(
        int id,
        UpdateScheduleExceptionDto dto);

    Task<bool> DeleteAsync(int id);
}