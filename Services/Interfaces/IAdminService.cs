using BookingSystem.Dtos.Admin;

namespace BookingSystem.Services.Interfaces;

public interface IAdminService
{
    Task<AdminResponse> RegisterAsync(AdminRegistrationRequest request, CancellationToken ct = default);
}
