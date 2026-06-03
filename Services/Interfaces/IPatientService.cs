using BookingSystem.Dtos.Auth;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Patient;

namespace BookingSystem.Services.Interfaces;

public interface IPatientService
{
    Task<AuthResponse> RegisterAsync(
        PatientRegistrationRequest request,
        CancellationToken ct = default
    );

    Task<PatientProfileResponse> GetProfileAsync(
        Guid patientId,
        CancellationToken ct = default
    );

    Task<PatientProfileResponse> UpdateProfileAsync(
        Guid patientId,
        UpdatePatientRequest request,
        CancellationToken ct = default
    );

    Task ChangePasswordAsync(
        Guid patientId,
        ChangePasswordRequest request,
        CancellationToken ct = default
    );

    Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(
        PatientSearchRequest request,
        CancellationToken ct = default
    );

    Task DeactivateAsync(
        Guid patientId,
        CancellationToken ct = default
    );

    Task ReactivateAsync(
        Guid patientId,
        CancellationToken ct = default
    );
}
