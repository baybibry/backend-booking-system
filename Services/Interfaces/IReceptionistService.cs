using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Patient;
using BookingSystem.Dtos.Receptionist;
using BookingSystem.Dtos.Schedule;

namespace BookingSystem.Services.Interfaces;

public interface IReceptionistService
{
    Task<ReceptionistProfileResponse> RegisterAsync(ReceptionistRegistrationRequest request, CancellationToken ct = default);
    Task<PagedResponse<ReceptionistSummaryResponse>> GetAllAsync(PaginationRequest request, CancellationToken ct = default);
    Task DeactivateAsync(Guid receptionistId, CancellationToken ct = default);
    Task ReactivateAsync(Guid receptionistId, CancellationToken ct = default);

    Task<ReceptionistProfileResponse> GetProfileAsync(Guid receptionistId, CancellationToken ct = default);
    Task<ReceptionistProfileResponse> UpdateProfileAsync(Guid receptionistId, UpdateReceptionistRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid receptionistId, ChangePasswordRequest request, CancellationToken ct = default);

    Task<PagedResponse<DoctorSummaryResponse>> GetDoctorsAsync(DoctorSearchRequest request, CancellationToken ct = default);
    Task<DoctorProfileResponse> GetDoctorAsync(Guid doctorId, CancellationToken ct = default);
    Task<List<ScheduleResponse>> GetDoctorSchedulesAsync(Guid doctorId, CancellationToken ct = default);
    Task<List<ScheduleWithDoctorResponse>> GetAllNonExpiredSchedulesAsync(CancellationToken ct = default);

    Task<PagedResponse<PatientSummaryResponse>> GetPatientsAsync(PatientSearchRequest request, CancellationToken ct = default);
    Task<PatientProfileResponse> GetPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<PagedResponse<AppointmentSummaryResponse>> GetPatientAppointmentsAsync(Guid patientId, AppointmentFilterRequest request, CancellationToken ct = default);

    Task<AppointmentResponse> AcceptAppointmentAsync(Guid receptionistId, Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentResponse> CancelAppointmentAsync(Guid receptionistId, Guid appointmentId, string? cancellationNote, CancellationToken ct = default);
    Task<AppointmentResponse> RescheduleAppointmentAsync(Guid receptionistId, Guid appointmentId, Guid newScheduleId, CancellationToken ct = default);
    Task<AppointmentResponse> MarkArrivedAsync(Guid receptionistId, Guid appointmentId, CancellationToken ct = default);
    Task<AppointmentResponse> CompleteAppointmentAsync(Guid receptionistId, Guid appointmentId, CancellationToken ct = default);
    Task<PagedResponse<AppointmentSummaryResponse>> GetAppointmentsAsync(AppointmentFilterRequest request, CancellationToken ct = default);
    Task<AppointmentResponse> GetAppointmentAsync(Guid appointmentId, CancellationToken ct = default);

    Task<ScheduleResponse> CreateScheduleForDoctorAsync(ReceptionistCreateScheduleRequest request, CancellationToken ct = default);
    Task<ScheduleResponse> ToggleBlockScheduleAsync(Guid scheduleId, CancellationToken ct = default);
}
