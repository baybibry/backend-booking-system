using BookingSystem.Data;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class SpecializationRepository(BookingContext context) : ISpecializationRepository
{
    public async Task<Specialization> CreateAsync(Specialization specialization, CancellationToken ct = default)
    {
        specialization.CreatedAt = DateTime.UtcNow;
        specialization.UpdatedAt = DateTime.UtcNow;
        context.Specializations.Add(specialization);
        await context.SaveChangesAsync(ct);
        return specialization;
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await context.Specializations.AnyAsync(
            s => s.SpecializationName.ToLower() == name.ToLower() && s.SpecializationId != excludeId,
            ct
        );

    public async Task<Specialization?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Specializations.FirstOrDefaultAsync(s => s.SpecializationId == id, ct);

    public async Task<Specialization> UpdateAsync(Guid id, UpdateSpecializationRequest request, CancellationToken ct = default)
    {
        var specialization = await context.Specializations.FirstOrDefaultAsync(s => s.SpecializationId == id, ct)
            ?? throw new KeyNotFoundException("Specialization not found.");

        if (request.SpecializationName is not null) specialization.SpecializationName = request.SpecializationName;
        specialization.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return specialization;
    }

    public async Task<Specialization> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var specialization = await context.Specializations.FirstOrDefaultAsync(s => s.SpecializationId == id, ct)
            ?? throw new KeyNotFoundException("Specialization not found.");

        var hasLinkedDoctors = await context.Doctors
            .AnyAsync(d => d.SpecializationId == id, ct);

        if (hasLinkedDoctors)
            throw new InvalidOperationException(
                "This specialization cannot be deleted because it is assigned to one or more doctors. " +
                "Please reassign those doctors to a different specialization first.");

        context.Specializations.Remove(specialization);
        await context.SaveChangesAsync(ct);
        return specialization;
    }

    public async Task<List<Specialization>> GetAllAsync(CancellationToken ct = default) =>
        await context.Specializations
            .OrderBy(s => s.SpecializationName)
            .AsNoTracking()
            .ToListAsync(ct);
}
