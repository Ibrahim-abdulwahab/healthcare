using Healthcare.Api.DTOs.PrescriptionItems;

namespace Healthcare.Api.Services.PrescriptionItems;

public interface IPrescriptionItemService
{
    Task<IEnumerable<PrescriptionItemDto>> GetAllAsync();

    Task<PrescriptionItemDto?> GetByIdAsync(int id);

    Task<PrescriptionItemDto> CreateAsync(
        CreatePrescriptionItemDto dto
    );

    Task<PrescriptionItemDto?> UpdateAsync(
        int id,
        UpdatePrescriptionItemDto dto
    );

    Task<bool> DeleteAsync(int id);
}