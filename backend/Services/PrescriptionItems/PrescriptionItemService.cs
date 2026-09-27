using Healthcare.Api.Security;
using Healthcare.Api.Data;
using Healthcare.Api.DTOs.PrescriptionItems;
using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Api.Services.PrescriptionItems;

public class PrescriptionItemService : IPrescriptionItemService
{
    private readonly HealthcareDbContext _context;
    private readonly ResourceAccess _access;

    public PrescriptionItemService(HealthcareDbContext context, ResourceAccess access)
    {
        _context = context;
        _access = access;
    }

    public async Task<IEnumerable<PrescriptionItemDto>> GetAllAsync()
    {
        return await _access.List<PrescriptionItem>()
            .Select(pi => new PrescriptionItemDto
            {
                Id = pi.Id,
                PrescriptionId = pi.PrescriptionId,
                ItemId = pi.ItemId,
                Dosage = pi.Dosage,
                Frequency = pi.Frequency,
                DurationDays = pi.DurationDays,
                Quantity = pi.Quantity,
                Route = pi.Route,
                Instructions = pi.Instructions,
                Notes = pi.Notes,
                CreatedAt = pi.CreatedAt,
                UpdatedAt = pi.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PrescriptionItemDto?> GetByIdAsync(int id)
    {
        return await _access.Query<PrescriptionItem>()
            .Where(pi => pi.Id == id)
            .Select(pi => new PrescriptionItemDto
            {
                Id = pi.Id,
                PrescriptionId = pi.PrescriptionId,
                ItemId = pi.ItemId,
                Dosage = pi.Dosage,
                Frequency = pi.Frequency,
                DurationDays = pi.DurationDays,
                Quantity = pi.Quantity,
                Route = pi.Route,
                Instructions = pi.Instructions,
                Notes = pi.Notes,
                CreatedAt = pi.CreatedAt,
                UpdatedAt = pi.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PrescriptionItemDto> CreateAsync(
        CreatePrescriptionItemDto dto)
    {
        await ValidateRelationshipsAsync(
            dto.PrescriptionId,
            dto.ItemId
        );

        var now = DateTime.UtcNow;

        var prescriptionItem = new PrescriptionItem
        {
            PrescriptionId = dto.PrescriptionId,
            ItemId = dto.ItemId,
            Dosage = dto.Dosage,
            Frequency = dto.Frequency,
            DurationDays = dto.DurationDays,
            Quantity = dto.Quantity,
            Route = dto.Route,
            Instructions = dto.Instructions,
            Notes = dto.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.PrescriptionItems.Add(prescriptionItem);
        await _context.SaveChangesAsync();

        return ToDto(prescriptionItem);
    }

    public async Task<PrescriptionItemDto?> UpdateAsync(
        int id,
        UpdatePrescriptionItemDto dto)
    {
        var prescriptionItem =
            await _context.PrescriptionItems.FindAsync(id);

        if (prescriptionItem is null)
            return null;

        await ValidateRelationshipsAsync(
            dto.PrescriptionId,
            dto.ItemId
        );

        prescriptionItem.PrescriptionId = dto.PrescriptionId;
        prescriptionItem.ItemId = dto.ItemId;
        prescriptionItem.Dosage = dto.Dosage;
        prescriptionItem.Frequency = dto.Frequency;
        prescriptionItem.DurationDays = dto.DurationDays;
        prescriptionItem.Quantity = dto.Quantity;
        prescriptionItem.Route = dto.Route;
        prescriptionItem.Instructions = dto.Instructions;
        prescriptionItem.Notes = dto.Notes;
        prescriptionItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToDto(prescriptionItem);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var prescriptionItem =
            await _context.PrescriptionItems.FindAsync(id);

        if (prescriptionItem is null)
            return false;

        _context.PrescriptionItems.Remove(prescriptionItem);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task ValidateRelationshipsAsync(
        int prescriptionId,
        int itemId)
    {
        var prescriptionExists = await _context.Prescriptions
            .AsNoTracking()
            .AnyAsync(p => p.Id == prescriptionId);

        if (!prescriptionExists)
        {
            throw new ArgumentException(
                $"Prescription with ID {prescriptionId} does not exist."
            );
        }

        var itemExists = await _context.Items
            .AsNoTracking()
            .AnyAsync(i => i.Id == itemId);

        if (!itemExists)
        {
            throw new ArgumentException(
                $"Item with ID {itemId} does not exist."
            );
        }
    }

    private static PrescriptionItemDto ToDto(
        PrescriptionItem prescriptionItem)
    {
        return new PrescriptionItemDto
        {
            Id = prescriptionItem.Id,
            PrescriptionId = prescriptionItem.PrescriptionId,
            ItemId = prescriptionItem.ItemId,
            Dosage = prescriptionItem.Dosage,
            Frequency = prescriptionItem.Frequency,
            DurationDays = prescriptionItem.DurationDays,
            Quantity = prescriptionItem.Quantity,
            Route = prescriptionItem.Route,
            Instructions = prescriptionItem.Instructions,
            Notes = prescriptionItem.Notes,
            CreatedAt = prescriptionItem.CreatedAt,
            UpdatedAt = prescriptionItem.UpdatedAt
        };
    }
}