using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;

namespace BookingSystem.Services.Interfaces;

public interface IDoctorService
{
    Task<DoctorProfileResponse> RegisterAsync(
        DoctorRegistrationRequest request,
        CancellationToken ct = default
    );

    Task<DoctorProfileResponse> GetProfileAsync(
        Guid doctorId,
        CancellationToken ct = default
    );

    Task<DoctorProfileResponse> UpdateProfileAsync(
        Guid doctorId,
        UpdateDoctorRequest request,
        CancellationToken ct = default
    );

    Task ChangePasswordAsync(
        Guid doctorId,
        ChangePasswordRequest request,
        CancellationToken ct = default
    );

    Task<PagedResponse<DoctorSummaryResponse>> SearchAsync(
        DoctorSearchRequest request,
        CancellationToken ct = default
    );

    Task<PagedResponse<DoctorSummaryResponse>> GetAllAsync(
        PaginationRequest request,
        CancellationToken ct = default
    );

    Task DeactivateAsync(
        Guid doctorId,
        CancellationToken ct = default
    );

    Task ReactivateAsync(
        Guid doctorId,
        CancellationToken ct = default
    );
}
