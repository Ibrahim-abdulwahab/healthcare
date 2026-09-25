using Healthcare.Api.DTOs.PaymentRecords;

namespace Healthcare.Api.Services.PaymentRecords;

public interface IPaymentRecordService
{
    Task<IEnumerable<PaymentRecordDto>> GetAllAsync();

    Task<PaymentRecordDto?> GetByIdAsync(int id);

    Task<PaymentRecordDto> CreateAsync(CreatePaymentRecordDto dto);

    Task<PaymentRecordDto?> UpdateAsync(
        int id,
        UpdatePaymentRecordDto dto
    );

    Task<bool> DeleteAsync(int id);
}