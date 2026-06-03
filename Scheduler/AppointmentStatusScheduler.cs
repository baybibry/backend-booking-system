using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;

namespace BookingSystem.Scheduler;

public class AppointmentStatusScheduler(
    IServiceProvider serviceProvider,
    ILogger<AppointmentStatusScheduler> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            TimeSpan midnight = DateTime.Today.AddDays(1) - DateTime.UtcNow;

            try
            {
                await Task.Delay(midnight, ct);
                await ClearExpiredBookings(ct);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task ClearExpiredBookings(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var appointmentRepo = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        List<Appointment> expiryBooks = await appointmentRepo.GetExpiryAppointmentsAsync(ct);

        await appointmentRepo.ConvertToExpiredAsync(expiryBooks, ct);
        logger.LogInformation("Auto-expired {Count} appointments.", expiryBooks.Count);
    }
}