using Healthcare.Api.DTOs.Consultations;

namespace Healthcare.Api.Services.Consultations;

public interface IConsultationService
{
    Task<IEnumerable<ConsultationDto>> GetAllAsync();

    Task<ConsultationDto?> GetByIdAsync(int id);

    Task<ConsultationDto> CreateAsync(CreateConsultationDto dto);

    Task<ConsultationDto?> UpdateAsync(
        int id,
        UpdateConsultationDto dto
    );

    Task<bool> DeleteAsync(int id);
}