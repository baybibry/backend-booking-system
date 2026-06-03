using BookingSystem.Dtos.Admin;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;

namespace BookingSystem.Services.Implementations;

public class AdminService(IAdminRepository adminRepo) : IAdminService
{
    public async Task<AdminResponse> RegisterAsync(AdminRegistrationRequest request, CancellationToken ct = default)
    {
        if (await adminRepo.ExistsByUsernameAsync(request.Username, ct))
            throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

        var admin = new Admin
        {
            AdminId = Guid.NewGuid(),
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await adminRepo.CreateAsync(admin, ct);

        return new AdminResponse
        {
            AdminId = created.AdminId,
            Username = created.Username,
            CreatedAt = created.CreatedAt
        };
    }
}
