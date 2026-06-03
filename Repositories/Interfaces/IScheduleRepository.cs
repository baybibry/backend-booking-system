using BookingSystem.Dtos.Schedule;
using BookingSystem.Entities;

namespace BookingSystem.Repositories.Interfaces;

public interface IScheduleRepository
{
    Task<bool> HasOverlapAsync(Guid doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? excludeScheduleId = null, CancellationToken ct = default);
    Task<Schedule> CreateAsync(Schedule schedule, CancellationToken ct = default);
    Task<Schedule> UpdateAsync(Guid scheduleId, UpdateScheduleRequest request, CancellationToken ct = default);
    Task<Schedule> ToggleBlockAsync(Guid scheduleId, CancellationToken ct = default);
    Task<Schedule> DeleteAsync(Guid scheduleId, CancellationToken ct = default);

    Task<Schedule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Schedule>> GetByDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task<List<Schedule>> GetAvailableByDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task<List<Schedule>> GetAllNonExpiredAsync(CancellationToken ct = default);
}
