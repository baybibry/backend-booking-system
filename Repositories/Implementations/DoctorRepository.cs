using BookingSystem.Data;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class DoctorRepository(BookingContext context) : IDoctorRepository
{
    public async Task<Doctor?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Doctors
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await context.Doctors.AnyAsync(d => d.Email == email, ct);

    public async Task<bool> LicenseNoExistsAsync(string licenseNo, CancellationToken ct = default) =>
        await context.Doctors.AnyAsync(d => d.LicenseNo == licenseNo, ct);

    public async Task<Doctor> CreateAsync(Doctor doctor, CancellationToken ct = default)
    {
        doctor.CreatedAt = DateTime.UtcNow;
        doctor.UpdatedAt = DateTime.UtcNow;
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync(ct);
        return doctor;
    }

    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Doctors
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.DoctorId == id, ct);

    public async Task<Doctor> UpdateAsync(Guid id, UpdateDoctorRequest request, CancellationToken ct = default)
    {
        var doctor = await context.Doctors
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.DoctorId == id, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        if (request.FirstName is not null && request.FirstName != doctor.FirstName ) doctor.FirstName = request.FirstName;
        if (request.MiddleName is not null && request.MiddleName != doctor.MiddleName) doctor.MiddleName = request.MiddleName;
        if (request.LastName is not null && request.LastName != doctor.LastName) doctor.LastName = request.LastName;
        if (request.Email is not null && request.Email != doctor.Email) doctor.Email = request.Email;
        if (request.Phone is not null && request.Phone != doctor.Phone) doctor.Phone = request.Phone;
        if (request.LicenseNo is not null && request.LicenseNo != doctor.LicenseNo) doctor.LicenseNo = request.LicenseNo;
        doctor.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return doctor;
    }

    public async Task<Doctor> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken ct = default)
    {
        var doctor = await context.Doctors
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.DoctorId == id, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        doctor.Password = hashedPassword;
        doctor.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return doctor;
    }

    public async Task<PagedResponse<DoctorSummaryResponse>> SearchAsync(DoctorSearchRequest request, CancellationToken ct = default)
    {
        var query = context.Doctors
            .Include(d => d.Specialization)
            .Where(d => !d.Deactivated)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(d =>
                d.FirstName.Contains(request.Search) ||
                d.LastName.Contains(request.Search));

        if (request.SpecializationId.HasValue)
            query = query.Where(d => d.SpecializationId == request.SpecializationId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(d => d.LastName)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(d => new DoctorSummaryResponse
            {
                DoctorId = d.DoctorId,
                FirstName = d.FirstName,
                MiddleName = d.MiddleName,
                LastName = d.LastName,
                Email = d.Email,
                Phone = d.Phone,
                Deactivated = d.Deactivated,
                Specialization = new SpecializationResponse
                {
                    SpecializationId = d.Specialization.SpecializationId,
                    SpecializationName = d.Specialization.SpecializationName
                }
            })
            .ToListAsync(ct);

        return new PagedResponse<DoctorSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<DoctorSummaryResponse>> GetAllAsync(PaginationRequest request, CancellationToken ct = default)
    {
        var query = context.Doctors
            .Include(d => d.Specialization)
            .AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(d => d.LastName)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(d => new DoctorSummaryResponse
            {
                DoctorId = d.DoctorId,
                FirstName = d.FirstName,
                MiddleName = d.MiddleName,
                LastName = d.LastName,
                Email = d.Email,
                Phone = d.Phone,
                Deactivated = d.Deactivated,
                Specialization = new SpecializationResponse
                {
                    SpecializationId = d.Specialization.SpecializationId,
                    SpecializationName = d.Specialization.SpecializationName
                }
            })
            .ToListAsync(ct);

        return new PagedResponse<DoctorSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount
        };
    }

    public async Task<Doctor> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == id, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        doctor.Deactivated = true;
        doctor.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return doctor;
    }

    public async Task<Doctor> ReactivateAsync(Guid id, CancellationToken ct = default)
    {
        var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == id, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        doctor.Deactivated = false;
        doctor.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return doctor;
    }
}
