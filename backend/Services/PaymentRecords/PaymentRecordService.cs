using Healthcare.Api.Data;
using Healthcare.Api.DTOs.PaymentRecords;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.PaymentRecords;

public class PaymentRecordService : IPaymentRecordService
{
    private readonly HealthcareDbContext _context;

    public PaymentRecordService(HealthcareDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentRecordDto>> GetAllAsync()
    {
        return await _context.PaymentRecords
            .AsNoTracking()
            .Select(p => new PaymentRecordDto
            {
                Id = p.Id,
                VisitId = p.VisitId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                CollectedBy = p.CollectedBy,
                PaidTo = p.PaidTo,
                PaymentStatus = p.PaymentStatus,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PaymentRecordDto?> GetByIdAsync(int id)
    {
        return await _context.PaymentRecords
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PaymentRecordDto
            {
                Id = p.Id,
                VisitId = p.VisitId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                CollectedBy = p.CollectedBy,
                PaidTo = p.PaidTo,
                PaymentStatus = p.PaymentStatus,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PaymentRecordDto> CreateAsync(
        CreatePaymentRecordDto dto)
    {
        await ValidateRelationshipsAsync(
            dto.VisitId,
            dto.CollectedBy
        );

        var now = DateTime.UtcNow;

        var paymentRecord = new PaymentRecord
        {
            VisitId = dto.VisitId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            CollectedBy = dto.CollectedBy,
            PaidTo = dto.PaidTo,
            PaymentStatus = dto.PaymentStatus,
            Notes = dto.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.PaymentRecords.Add(paymentRecord);
        await _context.SaveChangesAsync();

        return ToDto(paymentRecord);
    }

    public async Task<PaymentRecordDto?> UpdateAsync(
        int id,
        UpdatePaymentRecordDto dto)
    {
        var paymentRecord =
            await _context.PaymentRecords.FindAsync(id);

        if (paymentRecord is null)
            return null;

        await ValidateRelationshipsAsync(
            dto.VisitId,
            dto.CollectedBy
        );

        paymentRecord.VisitId = dto.VisitId;
        paymentRecord.Amount = dto.Amount;
        paymentRecord.PaymentMethod = dto.PaymentMethod;
        paymentRecord.CollectedBy = dto.CollectedBy;
        paymentRecord.PaidTo = dto.PaidTo;
        paymentRecord.PaymentStatus = dto.PaymentStatus;
        paymentRecord.Notes = dto.Notes;
        paymentRecord.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(paymentRecord);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var paymentRecord =
            await _context.PaymentRecords.FindAsync(id);

        if (paymentRecord is null)
            return false;

        _context.PaymentRecords.Remove(paymentRecord);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateRelationshipsAsync(
        int visitId,
        int collectedBy)
    {
        var visitExists = await _context.Visits
            .AsNoTracking()
            .AnyAsync(v => v.Id == visitId);

        if (!visitExists)
        {
            throw new ArgumentException(
                $"Visit with ID {visitId} does not exist."
            );
        }

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == collectedBy);

        if (!userExists)
        {
            throw new ArgumentException(
                $"User with ID {collectedBy} does not exist."
            );
        }
    }

    private static PaymentRecordDto ToDto(
        PaymentRecord paymentRecord)
    {
        return new PaymentRecordDto
        {
            Id = paymentRecord.Id,
            VisitId = paymentRecord.VisitId,
            Amount = paymentRecord.Amount,
            PaymentMethod = paymentRecord.PaymentMethod,
            CollectedBy = paymentRecord.CollectedBy,
            PaidTo = paymentRecord.PaidTo,
            PaymentStatus = paymentRecord.PaymentStatus,
            Notes = paymentRecord.Notes,
            CreatedAt = paymentRecord.CreatedAt,
            UpdatedAt = paymentRecord.UpdatedAt
        };
    }
}