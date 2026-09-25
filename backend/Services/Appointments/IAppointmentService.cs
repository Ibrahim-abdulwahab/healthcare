using Healthcare.Api.DTOs.Appointments;

namespace Healthcare.Api.Services.Appointments;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();

    Task<AppointmentDto?> GetByIdAsync(int id);

    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);

    Task<AppointmentDto?> UpdateAsync(
        int id,
        UpdateAppointmentDto dto
    );

    Task<bool> DeleteAsync(int id);
}