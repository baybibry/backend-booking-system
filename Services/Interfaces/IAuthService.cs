using BookingSystem.Dtos.Auth;

namespace BookingSystem.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> PatientLoginAsync(
        DefaultLoginRequest request,
        CancellationToken ct = default
    );

    Task<AuthResponse> DoctorLoginAsync(
        DefaultLoginRequest request,
        CancellationToken ct = default
    );

    Task<AuthResponse> AdminLoginAsync(
        AdminLoginRequest request,
        CancellationToken ct = default
    );

    Task<AuthResponse> ReceptionistLoginAsync(
        DefaultLoginRequest request,
        CancellationToken ct = default
    );

    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}
