using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Entities;

namespace BookingSystem.Repositories.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> LicenseNoExistsAsync(string licenseNo, CancellationToken ct = default);
    Task<Doctor> CreateAsync(Doctor doctor, CancellationToken ct = default);

    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Doctor> UpdateAsync(Guid id, UpdateDoctorRequest request, CancellationToken ct = default);
    Task<Doctor> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken ct = default);

    Task<PagedResponse<DoctorSummaryResponse>> SearchAsync(DoctorSearchRequest request, CancellationToken ct = default);

    Task<PagedResponse<DoctorSummaryResponse>> GetAllAsync(PaginationRequest request, CancellationToken ct = default);
    Task<Doctor> DeactivateAsync(Guid id, CancellationToken ct = default);
    Task<Doctor> ReactivateAsync(Guid id, CancellationToken ct = default);
}
