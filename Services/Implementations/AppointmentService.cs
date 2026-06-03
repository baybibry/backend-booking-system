using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Schedule;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Dtos.Patient;
using BookingSystem.Entities;
using BookingSystem.Enums;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;

namespace BookingSystem.Services.Implementations;

public class AppointmentService(
    IAppointmentRepository appointmentRepo,
    IScheduleRepository scheduleRepo
) : IAppointmentService
{
    public async Task<AppointmentResponse> BookAsync(
        Guid patientId,
        Guid doctorId,
        BookAppointmentRequest request,
        CancellationToken ct = default
    )
    {
        var schedule = await scheduleRepo.GetByIdAsync(request.ScheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        if (schedule.DoctorId != doctorId)
            throw new InvalidOperationException("Schedule does not belong to the specified doctor.");

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduleId = request.ScheduleId,
            Reason = request.Reason,
            Status = AppointmentStatus.Pending
        };

        var created = await appointmentRepo.BookAsync(appointment, ct);
        var full = await appointmentRepo.GetByIdAsync(created.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found after booking.");

        return MapToResponse(full);
    }

    public async Task<AppointmentResponse> CancelAsync(
        Guid patientId,
        CancelAppointmentRequest request,
        CancellationToken ct = default
    )
    {
        var cancelled = await appointmentRepo.CancelAsync(request.ScheduleId, patientId, request.CancellationNote, ct);
        var full = await appointmentRepo.GetByIdAsync(cancelled.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapToResponse(full);
    }

    public async Task<AppointmentResponse> RescheduleAsync(
        Guid patientId,
        RescheduleAppointmentRequest request,
        CancellationToken ct = default
    )
    {
        var rescheduled = await appointmentRepo.RescheduleAsync(request.ScheduleId, request.NewScheduleId, patientId, ct);
        var full = await appointmentRepo.GetByIdAsync(rescheduled.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapToResponse(full);
    }

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetPatientHistoryAsync(
        Guid patientId,
        AppointmentFilterRequest request,
        CancellationToken ct = default
    ) => await appointmentRepo.GetPatientHistoryAsync(patientId, request, ct);

    public async Task<AppointmentResponse> UpdateAsync(
        Guid appointmentId,
        Guid doctorId,
        UpdateAppointmentRequest request,
        CancellationToken ct = default
    )
    {
        var updated = await appointmentRepo.UpdateAsync(appointmentId, doctorId, request, ct);
        var full = await appointmentRepo.GetByIdAsync(updated.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapToResponse(full);
    }

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetDoctorAppointmentsAsync(
        Guid doctorId,
        AppointmentFilterRequest request,
        CancellationToken ct = default
    ) => await appointmentRepo.GetDoctorAppointmentsAsync(doctorId, request, ct);

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetAllAsync(
        AppointmentFilterRequest request,
        CancellationToken ct = default
    ) => await appointmentRepo.GetAllAsync(request, ct);

    public async Task<AppointmentResponse> GetByIdAsync(
        Guid appointmentId,
        CancellationToken ct = default
    )
    {
        var appointment = await appointmentRepo.GetByIdAsync(appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapToResponse(appointment);
    }

    private static AppointmentResponse MapToResponse(Appointment a) => new()
    {
        AppointmentId = a.AppointmentId,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        Diagnosis = a.Diagnosis,
        AutoCompleted = a.AutoCompleted,
        CancellationNote = a.CancellationNote,
        CancelledBy = a.CancelledBy,
        CreatedAt = a.CreatedAt,
        Patient = new PatientProfileResponse
        {
            PatientId = a.Patient.PatientId,
            FirstName = a.Patient.FirstName,
            MiddleName = a.Patient.MiddleName,
            LastName = a.Patient.LastName,
            Email = a.Patient.Email,
            Phone = a.Patient.Phone,
            Address = a.Patient.Address
        },
        Doctor = new DoctorSummaryResponse
        {
            DoctorId = a.Doctor.DoctorId,
            FirstName = a.Doctor.FirstName,
            MiddleName = a.Doctor.MiddleName,
            LastName = a.Doctor.LastName,
            Email = a.Doctor.Email,
            Phone = a.Doctor.Phone,
            Deactivated = a.Doctor.Deactivated,
            Specialization = new SpecializationResponse
            {
                SpecializationId = a.Doctor.Specialization.SpecializationId,
                SpecializationName = a.Doctor.Specialization.SpecializationName
            }
        },
        Schedule = new ScheduleResponse
        {
            ScheduleId = a.Schedule.ScheduleId,
            Date = a.Schedule.Date,
            StartTime = a.Schedule.StartTime,
            EndTime = a.Schedule.EndTime,
            IsBooked = a.Schedule.IsBooked,
            IsBlocked = a.Schedule.IsBlocked
        }
    };
}
