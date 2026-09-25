using Healthcare.Api.DTOs.Patients;

namespace Healthcare.Api.Services.Patients;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllAsync();

    Task<PatientDto?> GetByIdAsync(int id);

    Task<PatientDto> CreateAsync(CreatePatientDto dto);

    Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto);

    Task<bool> DeleteAsync(int id);
}