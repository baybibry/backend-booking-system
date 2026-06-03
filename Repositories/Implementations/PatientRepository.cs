using BookingSystem.Data;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Patient;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class PatientRepository(BookingContext context) : IPatientRepository
{
    public async Task<Patient?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Patients.FirstOrDefaultAsync(p => p.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await context.Patients.AnyAsync(p => p.Email == email, ct);

    public async Task<Patient> CreateAsync(Patient patient, CancellationToken ct = default)
    {
        patient.CreatedAt = DateTime.UtcNow;
        patient.UpdatedAt = DateTime.UtcNow;
        context.Patients.Add(patient);
        await context.SaveChangesAsync(ct);
        return patient;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Patients.FirstOrDefaultAsync(p => p.PatientId == id, ct);

    public async Task<Patient> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken ct = default)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == id, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (request.FirstName is not null) patient.FirstName = request.FirstName;
        if (request.MiddleName != patient.MiddleName) patient.MiddleName = request.MiddleName;
        if (request.LastName is not null) patient.LastName = request.LastName;
        if (request.Email is not null) patient.Email = request.Email;
        if (request.Phone is not null) patient.Phone = request.Phone;
        if (request.Address is not null) patient.Address = request.Address;
        patient.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return patient;
    }

    public async Task<Patient> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken ct = default)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == id, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        patient.Password = hashedPassword;
        patient.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return patient;
    }

    public async Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(PatientSearchRequest request, CancellationToken ct = default)
    {
        var query = context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = $"%{request.Search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, term) ||
                EF.Functions.Like(p.LastName, term) ||
                EF.Functions.Like(p.Email, term)
            );
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(p => new PatientSummaryResponse
            {
                PatientId = p.PatientId,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                Email = p.Email,
                Phone = p.Phone,
                Deactivated = p.Deactivated
            })
            .ToListAsync(ct);

        return new PagedResponse<PatientSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount
        };
    }

    public async Task<Patient> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == id, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        patient.Deactivated = true;
        patient.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return patient;
    }

    public async Task<Patient> ReactivateAsync(Guid id, CancellationToken ct = default)
    {
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == id, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        patient.Deactivated = false;
        patient.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return patient;
    }
}
