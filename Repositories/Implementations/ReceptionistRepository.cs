using BookingSystem.Data;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Receptionist;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class ReceptionistRepository(BookingContext context) : IReceptionistRepository
{
    public async Task<Receptionist?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Receptionists.FirstOrDefaultAsync(r => r.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await context.Receptionists.AnyAsync(r => r.Email == email, ct);

    public async Task<bool> EmployeeNoExistsAsync(string employeeNo, CancellationToken ct = default) =>
        await context.Receptionists.AnyAsync(r => r.EmployeeNo == employeeNo, ct);

    public async Task<Receptionist> CreateAsync(Receptionist receptionist, CancellationToken ct = default)
    {
        receptionist.CreatedAt = DateTime.UtcNow;
        receptionist.UpdatedAt = DateTime.UtcNow;
        context.Receptionists.Add(receptionist);
        await context.SaveChangesAsync(ct);
        return receptionist;
    }

    public async Task<Receptionist?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Receptionists.FirstOrDefaultAsync(r => r.ReceptionistId == id, ct);

    public async Task<Receptionist> UpdateAsync(Guid id, UpdateReceptionistRequest request, CancellationToken ct = default)
    {
        var receptionist = await context.Receptionists.FirstOrDefaultAsync(r => r.ReceptionistId == id, ct)
            ?? throw new KeyNotFoundException("Receptionist not found.");

        if (request.FirstName is not null) receptionist.FirstName = request.FirstName;
        if (request.MiddleName is not null) receptionist.MiddleName = request.MiddleName;
        if (request.LastName is not null) receptionist.LastName = request.LastName;
        if (request.Phone is not null) receptionist.Phone = request.Phone;
        if (request.EmployeeNo is not null) receptionist.EmployeeNo = request.EmployeeNo;

        if (request.Email is not null && request.Email != receptionist.Email)
        {
            if (await EmailExistsAsync(request.Email, ct))
                throw new InvalidOperationException("Email is already in use.");
            receptionist.Email = request.Email;
        }

        receptionist.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return receptionist;
    }

    public async Task<Receptionist> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken ct = default)
    {
        var receptionist = await context.Receptionists.FirstOrDefaultAsync(r => r.ReceptionistId == id, ct)
            ?? throw new KeyNotFoundException("Receptionist not found.");

        receptionist.Password = hashedPassword;
        receptionist.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return receptionist;
    }

    public async Task<PagedResponse<ReceptionistSummaryResponse>> GetAllAsync(PaginationRequest request, CancellationToken ct = default)
    {
        var query = context.Receptionists.AsNoTracking();
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(r => r.LastName).ThenBy(r => r.FirstName)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(r => new ReceptionistSummaryResponse
            {
                ReceptionistId = r.ReceptionistId,
                FirstName = r.FirstName,
                MiddleName = r.MiddleName,
                LastName = r.LastName,
                Email = r.Email,
                Phone = r.Phone,
                EmployeeNo = r.EmployeeNo,
                Deactivated = r.Deactivated
            })
            .ToListAsync(ct);

        return new PagedResponse<ReceptionistSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount
        };
    }

    public async Task<Receptionist> DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var receptionist = await context.Receptionists.FirstOrDefaultAsync(r => r.ReceptionistId == id, ct)
            ?? throw new KeyNotFoundException("Receptionist not found.");

        if (receptionist.Deactivated)
            throw new InvalidOperationException("Receptionist is already deactivated.");

        receptionist.Deactivated = true;
        receptionist.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return receptionist;
    }

    public async Task<Receptionist> ReactivateAsync(Guid id, CancellationToken ct = default)
    {
        var receptionist = await context.Receptionists.FirstOrDefaultAsync(r => r.ReceptionistId == id, ct)
            ?? throw new KeyNotFoundException("Receptionist not found.");

        if (!receptionist.Deactivated)
            throw new InvalidOperationException("Receptionist is already active.");

        receptionist.Deactivated = false;
        receptionist.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return receptionist;
    }
}
