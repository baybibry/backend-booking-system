using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Receptionist;
using BookingSystem.Entities;

namespace BookingSystem.Repositories.Interfaces;

public interface IReceptionistRepository
{
    Task<Receptionist?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> EmployeeNoExistsAsync(string employeeNo, CancellationToken ct = default);
    Task<Receptionist> CreateAsync(Receptionist receptionist, CancellationToken ct = default);

    Task<Receptionist?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Receptionist> UpdateAsync(Guid id, UpdateReceptionistRequest request, CancellationToken ct = default);
    Task<Receptionist> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken ct = default);

    Task<PagedResponse<ReceptionistSummaryResponse>> GetAllAsync(PaginationRequest request, CancellationToken ct = default);
    Task<Receptionist> DeactivateAsync(Guid id, CancellationToken ct = default);
    Task<Receptionist> ReactivateAsync(Guid id, CancellationToken ct = default);
}
