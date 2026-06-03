using BookingSystem.Entities;

namespace BookingSystem.Repositories.Interfaces;

public interface IAdminRepository
{
    Task<Admin?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Admin?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<Admin> CreateAsync(Admin admin, CancellationToken ct = default);
}
