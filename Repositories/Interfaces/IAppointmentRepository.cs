using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Entities;

namespace BookingSystem.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment> BookAsync(Appointment appointment, CancellationToken ct = default);
    Task<Appointment> CancelAsync(Guid scheduleId, Guid patientId, string? cancellationNote, CancellationToken ct = default);
    Task<Appointment> RescheduleAsync(Guid scheduleId, Guid newScheduleId, Guid patientId, CancellationToken ct = default);
    Task<PagedResponse<AppointmentSummaryResponse>> GetPatientHistoryAsync(Guid patientId, AppointmentFilterRequest request, CancellationToken ct = default);

    Task<Appointment> UpdateAsync(Guid appointmentId, Guid doctorId, UpdateAppointmentRequest request, CancellationToken ct = default);
    Task<PagedResponse<AppointmentSummaryResponse>> GetDoctorAppointmentsAsync(Guid doctorId, AppointmentFilterRequest request, CancellationToken ct = default);

    Task<Appointment> ReceptionistConfirmAsync(Guid appointmentId, Guid receptionistId, CancellationToken ct = default);
    Task<Appointment> ReceptionistCancelAsync(Guid appointmentId, Guid receptionistId, string? cancellationNote, CancellationToken ct = default);
    Task<Appointment> ReceptionistRescheduleAsync(Guid appointmentId, Guid newScheduleId, Guid receptionistId, CancellationToken ct = default);
    Task<Appointment> MarkAsArrivedAsync(Guid appointmentId, Guid receptionistId, CancellationToken ct = default);
    Task<Appointment> ReceptionistCompleteAsync(Guid appointmentId, Guid receptionistId, CancellationToken ct = default);

    Task<PagedResponse<AppointmentSummaryResponse>> GetAllAsync(AppointmentFilterRequest request, CancellationToken ct = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    Task<List<Appointment>> GetExpiryAppointmentsAsync(CancellationToken ct = default);
    Task ConvertToExpiredAsync(List<Appointment> expiryAppointments, CancellationToken ct = default);
}
