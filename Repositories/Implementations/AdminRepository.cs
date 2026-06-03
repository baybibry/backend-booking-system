using BookingSystem.Data;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class AdminRepository(BookingContext context) : IAdminRepository
{
    public async Task<Admin?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Admins.FirstOrDefaultAsync(a => a.AdminId == id, ct);

    public async Task<Admin?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await context.Admins.FirstOrDefaultAsync(a => a.Username == username, ct);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
        await context.Admins.AnyAsync(a => a.Username == username, ct);

    public async Task<Admin> CreateAsync(Admin admin, CancellationToken ct = default)
    {
        context.Admins.Add(admin);
        await context.SaveChangesAsync(ct);
        return admin;
    }
}
