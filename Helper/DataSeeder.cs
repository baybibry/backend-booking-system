using BookingSystem.Data;
using BookingSystem.Entities;

namespace BookingSystem.Helper;

public static class DataSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingContext>();

        if (context.Admins.Any()) return;

        var admin = new Admin
        {
            AdminId = Guid.NewGuid(),
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        Console.WriteLine("Default admin seeded — Username: admin | Password: Admin@123");
    }
}
