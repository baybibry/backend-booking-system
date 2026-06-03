using BookingSystem.Concurrency;
using BookingSystem.Data;
using BookingSystem.Dtos.Schedule;
using BookingSystem.Entities;
using BookingSystem.Enums;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class ScheduleRepository(
    BookingContext context,
    INotificationRepository notificationRepo,
    IScheduleLock scheduleLock
) : IScheduleRepository
{
    public async Task<bool> HasOverlapAsync(
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeScheduleId = null,
        CancellationToken ct = default
    ) =>
        await context.Schedules.AnyAsync(s =>
            s.DoctorId == doctorId &&
            s.Date == date &&
            s.ScheduleId != excludeScheduleId &&
            s.StartTime < endTime &&
            s.EndTime > startTime,
            ct
        );

    // POST /api/doctor/schedules | POST /api/receptionist/schedules
    public async Task<Schedule> CreateAsync(Schedule schedule, CancellationToken ct = default)
    {
        schedule.CreatedAt = DateTime.UtcNow;
        schedule.UpdatedAt = DateTime.UtcNow;
        context.Schedules.Add(schedule);
        await context.SaveChangesAsync(ct);
        return schedule;
    }

    // PUT /api/doctor/schedules/{scheduleId}
    public async Task<Schedule> UpdateAsync(Guid scheduleId, UpdateScheduleRequest request, CancellationToken ct = default)
    {
        using var _ = await scheduleLock.AcquireAsync(scheduleId, ct);

        var schedule = await context.Schedules
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        if (schedule.IsBlocked)
            throw new InvalidOperationException("Schedule slot is blocked.");

        if (request.Date is not null) schedule.Date = request.Date.Value;
        if (request.StartTime is not null) schedule.StartTime = request.StartTime.Value;
        if (request.EndTime is not null) schedule.EndTime = request.EndTime.Value;
        schedule.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        var activeAppointment = schedule.Appointments
            .FirstOrDefault(a => a.Status != AppointmentStatus.Cancelled);

        if (schedule.IsBooked && activeAppointment is not null)
        {
            await notificationRepo.CreateAsync(new Notification
            {
                UserId = activeAppointment.PatientId,
                UserType = UserRole.Patient,
                Subject = "Appointment Rescheduled by Doctor",
                Content = $"Your appointment has been moved to {request.Date:MMMM d, yyyy} at {request.StartTime}.",
                Type = NotificationType.AppointmentRescheduled
            }, ct);
        }

        return schedule;
    }

    // PUT /api/doctor/schedules/{scheduleId}/toggle-block | PUT /api/receptionist/schedules/{scheduleId}/toggle-block
    public async Task<Schedule> ToggleBlockAsync(Guid scheduleId, CancellationToken ct = default)
    {
        using var _ = await scheduleLock.AcquireAsync(scheduleId, ct);

        var schedule = await context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId, ct)
                       ?? throw new KeyNotFoundException("Schedule not found.");

        schedule.IsBlocked = !schedule.IsBlocked;
        schedule.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return schedule;
    }

    // DELETE /api/doctor/schedules/{scheduleId}
    public async Task<Schedule> DeleteAsync(Guid scheduleId, CancellationToken ct = default)
    {
        using var _ = await scheduleLock.AcquireAsync(scheduleId, ct);

        var schedule = await context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        if (schedule.IsBooked)
            throw new InvalidOperationException("Cannot delete a schedule slot that is already booked.");

        context.Schedules.Remove(schedule);
        await context.SaveChangesAsync(ct);
        return schedule;
    }

    public async Task<Schedule?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Schedules
            .Include(s => s.Doctor)
            .FirstOrDefaultAsync(s => s.ScheduleId == id, ct);

    public async Task<List<Schedule>> GetByDoctorAsync(Guid doctorId, CancellationToken ct = default) =>
        await context.Schedules
            .Where(s => s.DoctorId == doctorId)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<List<Schedule>> GetAvailableByDoctorAsync(Guid doctorId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Schedules
            .Where(s =>
                s.DoctorId == doctorId &&
                !s.IsBooked &&
                !s.IsBlocked &&
                s.Date >= today
            )
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<List<Schedule>> GetAllNonExpiredAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Schedules
            .Include(s => s.Doctor)
            .Where(s => s.Date >= today)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
