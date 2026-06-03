using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;

namespace BookingSystem.Services.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentResponse> BookAsync(
        Guid patientId,
        Guid doctorId,
        BookAppointmentRequest request,
        CancellationToken ct = default
    );

    Task<AppointmentResponse> CancelAsync(
        Guid patientId,
        CancelAppointmentRequest request,
        CancellationToken ct = default
    );

    Task<AppointmentResponse> RescheduleAsync(
        Guid patientId,
        RescheduleAppointmentRequest request,
        CancellationToken ct = default
    );

    Task<PagedResponse<AppointmentSummaryResponse>> GetPatientHistoryAsync(
        Guid patientId,
        AppointmentFilterRequest request,
        CancellationToken ct = default
    );

    Task<AppointmentResponse> UpdateAsync(
        Guid appointmentId,
        Guid doctorId,
        UpdateAppointmentRequest request,
        CancellationToken ct = default
    );

    Task<PagedResponse<AppointmentSummaryResponse>> GetDoctorAppointmentsAsync(
        Guid doctorId,
        AppointmentFilterRequest request,
        CancellationToken ct = default
    );

    Task<PagedResponse<AppointmentSummaryResponse>> GetAllAsync(
        AppointmentFilterRequest request,
        CancellationToken ct = default
    );

    Task<AppointmentResponse> GetByIdAsync(
        Guid appointmentId,
        CancellationToken ct = default
    );
}
