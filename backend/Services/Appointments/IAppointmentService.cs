using Healthcare.Api.DTOs.Appointments;

namespace Healthcare.Api.Services.Appointments;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();

    Task<List<AppointmentDto>> GetByDoctorIdAsync(int doctorId);

    Task<AppointmentDto?> GetByIdAsync(int id);

    Task<List<AppointmentDto>> GetByStaffUserIdAsync(int staffUserId);

    Task<bool> StaffCanManageDoctorAsync(
    int staffUserId,
    int doctorId);

    Task<AppointmentDto> CreateAsync(
    CreateAppointmentDto dto,
    int createdBy);

    Task<AppointmentDto?> UpdateAsync(
        int id,
        UpdateAppointmentDto dto
    );

    Task<AppointmentDto?> ChangeStatusAsync(int id, ChangeAppointmentStatusDto dto);
    Task<AppointmentDto?> RescheduleAsync(int id, RescheduleAppointmentDto dto);
    Task<bool> DeleteAsync(int id);
}