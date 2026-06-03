using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Patient;
using BookingSystem.Entities;

namespace BookingSystem.Repositories.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<Patient> CreateAsync(Patient patient, CancellationToken ct = default);

    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Patient> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken ct = default);
    Task<Patient> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken ct = default);

    Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(PatientSearchRequest request, CancellationToken ct = default);
    Task<Patient> DeactivateAsync(Guid id, CancellationToken ct = default);
    Task<Patient> ReactivateAsync(Guid id, CancellationToken ct = default);
}
