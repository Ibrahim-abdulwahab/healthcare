using Healthcare.Api.DTOs.ScheduleExceptions;

namespace Healthcare.Api.Services.ScheduleExceptions;

public interface IScheduleExceptionService
{
    Task<List<ScheduleExceptionDto>> GetAllAsync();

    Task<ScheduleExceptionDto?> GetByIdAsync(int id);

    Task<ScheduleExceptionDto> CreateAsync(
        CreateScheduleExceptionDto dto);

    Task<ScheduleExceptionDto?> UpdateAsync(
        int id,
        UpdateScheduleExceptionDto dto);

    Task<bool> DeleteAsync(int id);
}