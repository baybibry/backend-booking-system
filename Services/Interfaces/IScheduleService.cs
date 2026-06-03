using BookingSystem.Dtos.Schedule;

namespace BookingSystem.Services.Interfaces;

public interface IScheduleService
{
    Task<ScheduleResponse> CreateAsync(
        Guid doctorId,
        CreateScheduleRequest request,
        CancellationToken ct = default
    );

    Task<ScheduleResponse> UpdateAsync(
        Guid doctorId,
        Guid scheduleId,
        UpdateScheduleRequest request,
        CancellationToken ct = default
    );

    Task<ScheduleResponse> ToggleBlockAsync(
        Guid doctorId,
        Guid scheduleId,
        CancellationToken ct = default
    );

    Task<ScheduleResponse> DeleteAsync(
        Guid doctorId,
        Guid scheduleId,
        CancellationToken ct = default
    );

    Task<ScheduleResponse> GetByIdAsync(
        Guid scheduleId,
        CancellationToken ct = default
    );

    Task<List<ScheduleResponse>> GetByDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default
    );

    Task<List<ScheduleResponse>> GetAvailableByDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default
    );
}
