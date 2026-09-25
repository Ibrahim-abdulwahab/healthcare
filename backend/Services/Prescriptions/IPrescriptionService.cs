using Healthcare.Api.DTOs.Prescriptions;

namespace Healthcare.Api.Services.Prescriptions;

public interface IPrescriptionService
{
    Task<IEnumerable<PrescriptionDto>> GetAllAsync();

    Task<PrescriptionDto?> GetByIdAsync(int id);

    Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto);

    Task<PrescriptionDto?> UpdateAsync(
        int id,
        UpdatePrescriptionDto dto
    );

    Task<bool> DeleteAsync(int id);
}