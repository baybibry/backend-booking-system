using BookingSystem.Dtos.Schedule;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;

namespace BookingSystem.Services.Implementations;

public class ScheduleService(IScheduleRepository scheduleRepo) : IScheduleService
{
    public async Task<ScheduleResponse> CreateAsync(
        Guid doctorId,
        CreateScheduleRequest request,
        CancellationToken ct = default
    )
    {
        if (await scheduleRepo.HasOverlapAsync(doctorId, request.Date, request.StartTime, request.EndTime, ct: ct))
            throw new InvalidOperationException("A schedule slot already exists that overlaps with the requested date and time.");

        var schedule = new Schedule
        {
            DoctorId = doctorId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var created = await scheduleRepo.CreateAsync(schedule, ct);
        return MapToResponse(created);
    }

    public async Task<ScheduleResponse> UpdateAsync(
        Guid doctorId,
        Guid scheduleId,
        UpdateScheduleRequest request,
        CancellationToken ct = default
    )
    {
        var schedule = await scheduleRepo.GetByIdAsync(scheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        if (schedule.DoctorId != doctorId)
            throw new UnauthorizedAccessException("You are not authorized to update this schedule.");

        var newDate = request.Date ?? schedule.Date;
        var newStartTime = request.StartTime ?? schedule.StartTime;
        var newEndTime = request.EndTime ?? schedule.EndTime;

        if (await scheduleRepo.HasOverlapAsync(doctorId, newDate, newStartTime, newEndTime, excludeScheduleId: scheduleId, ct: ct))
            throw new InvalidOperationException("A schedule slot already exists that overlaps with the requested date and time.");

        var updated = await scheduleRepo.UpdateAsync(scheduleId, request, ct);
        return MapToResponse(updated);
    }

    public async Task<ScheduleResponse> ToggleBlockAsync(
        Guid doctorId,
        Guid scheduleId,
        CancellationToken ct = default
    )
    {
        var schedule = await scheduleRepo.GetByIdAsync(scheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        if (schedule.DoctorId != doctorId)
            throw new UnauthorizedAccessException("You are not authorized to modify this schedule.");

        var updated = await scheduleRepo.ToggleBlockAsync(scheduleId, ct);
        return MapToResponse(updated);
    }

    public async Task<ScheduleResponse> DeleteAsync(
        Guid doctorId,
        Guid scheduleId,
        CancellationToken ct = default
    )
    {
        var schedule = await scheduleRepo.GetByIdAsync(scheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        if (schedule.DoctorId != doctorId)
            throw new UnauthorizedAccessException("You are not authorized to delete this schedule.");

        var deleted = await scheduleRepo.DeleteAsync(scheduleId, ct);
        return MapToResponse(deleted);
    }

    public async Task<ScheduleResponse> GetByIdAsync(
        Guid scheduleId,
        CancellationToken ct = default
    )
    {
        var schedule = await scheduleRepo.GetByIdAsync(scheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        return MapToResponse(schedule);
    }

    public async Task<List<ScheduleResponse>> GetByDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default
    )
    {
        var schedules = await scheduleRepo.GetByDoctorAsync(doctorId, ct);
        return schedules.Select(MapToResponse).ToList();
    }

    public async Task<List<ScheduleResponse>> GetAvailableByDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default
    )
    {
        var schedules = await scheduleRepo.GetAvailableByDoctorAsync(doctorId, ct);
        return schedules.Select(MapToResponse).ToList();
    }

    private static ScheduleResponse MapToResponse(Schedule s) => new()
    {
        ScheduleId = s.ScheduleId,
        Date = s.Date,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        IsBooked = s.IsBooked,
        IsBlocked = s.IsBlocked
    };
}
