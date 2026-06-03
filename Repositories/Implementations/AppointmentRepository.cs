using BookingSystem.Concurrency;
using BookingSystem.Data;
using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Patient;
using BookingSystem.Dtos.Schedule;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Entities;
using BookingSystem.Enums;
using BookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Repositories.Implementations;

public class AppointmentRepository(
    BookingContext context,
    INotificationRepository notificationRepo,
    IScheduleLock scheduleLock,
    IAppointmentLock appointmentLock
) : IAppointmentRepository
{
    // POST /api/patient/appointments/{doctorId}
    public async Task<Appointment> BookAsync(Appointment appointment, CancellationToken ct = default)
    {
        using var _ = await scheduleLock.AcquireAsync(appointment.ScheduleId, ct);

        var schedule = await context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == appointment.ScheduleId, ct)
                       ?? throw new KeyNotFoundException("Schedule not found.");

        if (schedule.IsBooked)
            throw new InvalidOperationException("Schedule slot is already booked.");

        if (schedule.IsBlocked)
            throw new InvalidOperationException("Schedule slot is blocked.");

        var patient = await context.Patients.FirstOrDefaultAsync(p => p.PatientId == appointment.PatientId, ct)
                      ?? throw new KeyNotFoundException("Patient not found.");

        schedule.IsBooked = true;
        schedule.UpdatedAt = DateTime.UtcNow;
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync(ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "New Appointment",
            Content = $"New appointment has been booked for {patient.FirstName} {patient.LastName}.",
            Type = NotificationType.AppointmentBooked
        }, ct);

        await NotifyReceptionistsAsync(
            "New Appointment",
            $"{patient.FirstName} {patient.LastName} has booked an appointment.",
            NotificationType.AppointmentBooked, ct);

        return appointment;
    }

    // PUT /api/patient/appointments/cancel
    public async Task<Appointment> CancelAsync(Guid scheduleId, Guid patientId, string? cancellationNote, CancellationToken ct = default)
    {
        var appointment = await context.Appointments
            .Include(a => a.Schedule)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.ScheduleId == scheduleId && a.PatientId == patientId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        using var _ = await appointmentLock.AcquireAsync(appointment.AppointmentId, ct);

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("A completed appointment cannot be cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledBy = CancelledBy.Patient;
        appointment.CancellationNote = cancellationNote;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.Schedule.IsBooked = false;
        appointment.Schedule.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        var noteText = string.IsNullOrWhiteSpace(cancellationNote) ? "" : $" Reason: {cancellationNote}";
        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "Appointment Cancelled",
            Content = $"{appointment.Patient.FirstName} {appointment.Patient.LastName} has cancelled their appointment.{noteText}",
            Type = NotificationType.AppointmentCancelled
        }, ct);

        await NotifyReceptionistsAsync(
            "Appointment Cancelled",
            $"{appointment.Patient.FirstName} {appointment.Patient.LastName} has cancelled their appointment with Dr. {appointment.Doctor.LastName}.{noteText}",
            NotificationType.AppointmentCancelled, ct);

        return appointment;
    }

    // PUT /api/patient/appointments/reschedule
    public async Task<Appointment> RescheduleAsync(Guid scheduleId, Guid newScheduleId, Guid patientId, CancellationToken ct = default)
    {
        using var _ = await scheduleLock.AcquirePairAsync(scheduleId, newScheduleId, ct);

        var appointment = await context.Appointments
            .Include(a => a.Schedule)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.ScheduleId == scheduleId && a.PatientId == patientId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new InvalidOperationException("A cancelled or completed appointment cannot be rescheduled.");

        if (scheduleId == newScheduleId)
            throw new InvalidOperationException("New schedule must be different from the current one.");

        var newSchedule = await context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == newScheduleId, ct)
                          ?? throw new KeyNotFoundException("New schedule not found.");

        if (newSchedule.DoctorId != appointment.DoctorId)
            throw new InvalidOperationException("New schedule does not belong to the appointment's doctor.");

        if (newSchedule.IsBooked)
            throw new InvalidOperationException("New schedule slot is already booked.");

        if (newSchedule.IsBlocked)
            throw new InvalidOperationException("New schedule slot is blocked.");

        appointment.Schedule.IsBooked = false;
        appointment.Schedule.UpdatedAt = DateTime.UtcNow;

        newSchedule.IsBooked = true;
        newSchedule.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        await context.Appointments
            .Where(a => a.AppointmentId == appointment.AppointmentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.ScheduleId, newScheduleId)
                .SetProperty(a => a.Status, AppointmentStatus.Pending)
                .SetProperty(a => a.UpdatedAt, DateTime.UtcNow), ct);

        appointment.ScheduleId = newScheduleId;
        appointment.Status = AppointmentStatus.Pending;
        appointment.UpdatedAt = DateTime.UtcNow;

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "Appointment Rescheduled",
            Content = $"{appointment.Patient.FirstName} {appointment.Patient.LastName} has rescheduled their appointment to {newSchedule.Date:MMMM d, yyyy} at {newSchedule.StartTime}.",
            Type = NotificationType.AppointmentRescheduled
        }, ct);

        await NotifyReceptionistsAsync(
            "Appointment Rescheduled",
            $"{appointment.Patient.FirstName} {appointment.Patient.LastName} has rescheduled their appointment to {newSchedule.Date:MMMM d, yyyy} at {newSchedule.StartTime}.",
            NotificationType.AppointmentRescheduled, ct);

        return appointment;
    }

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetPatientHistoryAsync(Guid patientId, AppointmentFilterRequest request, CancellationToken ct = default)
    {
        var query = context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .Where(a => a.PatientId == patientId)
            .AsNoTracking();

        if (request.TodayOnly)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayScheduleIds = context.Schedules
                .Where(s => s.Date == today)
                .Select(s => s.ScheduleId);
            query = query.Where(a => todayScheduleIds.Contains(a.ScheduleId));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Schedule.Date).ThenBy(a => a.Schedule.StartTime)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(a => MapToSummary(a))
            .ToListAsync(ct);

        return new PagedResponse<AppointmentSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount
        };
    }

    // PUT /api/doctor/appointments/{appointmentId} | /complete | /confirm
    public async Task<Appointment> UpdateAsync(Guid appointmentId, Guid doctorId, UpdateAppointmentRequest request, CancellationToken ct = default)
    {
        using var _ = await appointmentLock.AcquireAsync(appointmentId, ct);

        var appointment = await context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.DoctorId != doctorId)
            throw new UnauthorizedAccessException("You are not allowed to update this appointment.");

        var statusChanged = false;
        if (request.Status is { } newStatus && newStatus != appointment.Status)
        {
            if (!IsValidTransition(appointment.Status, newStatus))
                throw new InvalidOperationException(
                    $"Cannot change appointment status from {appointment.Status} to {newStatus}.");

            if (newStatus == AppointmentStatus.Cancelled)
            {
                appointment.CancelledBy = CancelledBy.Doctor;
                appointment.CancellationNote = request.CancellationNote;
                appointment.Schedule.IsBooked = false;
                appointment.Schedule.UpdatedAt = DateTime.UtcNow;
            }

            appointment.Status = newStatus;
            statusChanged = true;
        }

        if (request.Notes is not null) appointment.Notes = request.Notes;
        if (request.Diagnosis is not null) appointment.Diagnosis = request.Diagnosis;
        appointment.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        if (!statusChanged) return appointment;

        (string subject, string content, NotificationType type) = appointment.Status switch
        {
            AppointmentStatus.Pending => (
                "Appointment Pending",
                $"Your appointment with Dr. {appointment.Doctor.LastName} is awaiting confirmation.",
                NotificationType.General),
            AppointmentStatus.Confirmed => (
                "Appointment Confirmed",
                $"Dr. {appointment.Doctor.LastName} has confirmed your appointment.",
                NotificationType.AppointmentConfirmed),
            AppointmentStatus.Cancelled => (
                "Appointment Cancelled",
                $"Dr. {appointment.Doctor.LastName} has cancelled your appointment." +
                (string.IsNullOrWhiteSpace(appointment.CancellationNote) ? "" : $" Reason: {appointment.CancellationNote}"),
                NotificationType.AppointmentCancelled),
            AppointmentStatus.Completed => (
                "Appointment Completed",
                $"Your appointment with Dr. {appointment.Doctor.LastName} has been marked as completed.",
                NotificationType.AppointmentCompleted),
            _ => (
                "Appointment Update",
                $"Your appointment with Dr. {appointment.Doctor.LastName} has been updated.",
                NotificationType.General)
        };

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.PatientId,
            UserType = UserRole.Patient,
            Subject = subject,
            Content = content,
            Type = type
        }, ct);

        var receptionistContent = appointment.Status switch
        {
            AppointmentStatus.Confirmed =>
                $"Dr. {appointment.Doctor.LastName} has confirmed the appointment for {appointment.Patient.FirstName} {appointment.Patient.LastName}.",
            AppointmentStatus.Cancelled =>
                $"Dr. {appointment.Doctor.LastName} has cancelled the appointment for {appointment.Patient.FirstName} {appointment.Patient.LastName}." +
                (string.IsNullOrWhiteSpace(appointment.CancellationNote) ? "" : $" Reason: {appointment.CancellationNote}"),
            AppointmentStatus.Completed =>
                $"Dr. {appointment.Doctor.LastName} has completed the appointment for {appointment.Patient.FirstName} {appointment.Patient.LastName}.",
            _ => null
        };

        if (receptionistContent is not null)
            await NotifyReceptionistsAsync(subject, receptionistContent, type, ct);

        return appointment;
    }

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetDoctorAppointmentsAsync(Guid doctorId, AppointmentFilterRequest request, CancellationToken ct = default)
    {
        var query = context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .Where(a => a.DoctorId == doctorId)
            .AsNoTracking();

        if (request.TodayOnly)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayScheduleIds = context.Schedules
                .Where(s => s.Date == today)
                .Select(s => s.ScheduleId);
            query = query.Where(a => todayScheduleIds.Contains(a.ScheduleId));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Schedule.Date).ThenBy(a => a.Schedule.StartTime)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(a => MapToSummary(a))
            .ToListAsync(ct);

        return new PagedResponse<AppointmentSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetAllAsync(AppointmentFilterRequest request, CancellationToken ct = default)
    {
        var query = context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .AsNoTracking();

        if (request.TodayOnly)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayScheduleIds = context.Schedules
                .Where(s => s.Date == today)
                .Select(s => s.ScheduleId);
            query = query.Where(a => todayScheduleIds.Contains(a.ScheduleId));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.Schedule.Date).ThenBy(a => a.Schedule.StartTime)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(a => MapToSummary(a))
            .ToListAsync(ct);

        return new PagedResponse<AppointmentSummaryResponse>
        {
            Items = items,
            Page = request.Page,
            Size = request.Size,
            TotalCount = totalCount
        };
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.AppointmentId == id, ct);

    // PUT /api/receptionist/appointments/{appointmentId}/accept
    public async Task<Appointment> ReceptionistConfirmAsync(Guid appointmentId, Guid receptionistId, CancellationToken ct = default)
    {
        using var _ = await appointmentLock.AcquireAsync(appointmentId, ct);

        var appointment = await context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Pending)
            throw new InvalidOperationException("Only pending appointments can be confirmed.");

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.ReceptionistId = receptionistId;
        appointment.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "Appointment Accepted by Receptionist",
            Content = $"Receptionist has accepted the booking for {appointment.Patient.FirstName} {appointment.Patient.LastName}.",
            Type = NotificationType.AppointmentAcceptedByReceptionist
        }, ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.PatientId,
            UserType = UserRole.Patient,
            Subject = "Appointment Confirmed",
            Content = $"Your appointment with Dr. {appointment.Doctor.LastName} has been confirmed.",
            Type = NotificationType.AppointmentConfirmed
        }, ct);

        return appointment;
    }

    // PUT /api/receptionist/appointments/{appointmentId}/cancel
    public async Task<Appointment> ReceptionistCancelAsync(Guid appointmentId, Guid receptionistId, string? cancellationNote, CancellationToken ct = default)
    {
        using var _ = await appointmentLock.AcquireAsync(appointmentId, ct);

        var appointment = await context.Appointments
            .Include(a => a.Schedule)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status == AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Appointment is already cancelled.");

        if (appointment.Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("A completed appointment cannot be cancelled.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledBy = CancelledBy.Receptionist;
        appointment.CancellationNote = cancellationNote;
        appointment.ReceptionistId = receptionistId;
        appointment.UpdatedAt = DateTime.UtcNow;
        appointment.Schedule.IsBooked = false;
        appointment.Schedule.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        var noteText = string.IsNullOrWhiteSpace(cancellationNote) ? "" : $" Reason: {cancellationNote}";
        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.PatientId,
            UserType = UserRole.Patient,
            Subject = "Appointment Cancelled",
            Content = $"Your appointment with Dr. {appointment.Doctor.LastName} has been cancelled by the receptionist.{noteText}",
            Type = NotificationType.AppointmentCancelled
        }, ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "Appointment Cancelled",
            Content = $"The appointment for {appointment.Patient.FirstName} {appointment.Patient.LastName} has been cancelled by the receptionist.{noteText}",
            Type = NotificationType.AppointmentCancelled
        }, ct);

        return appointment;
    }

    // PUT /api/receptionist/appointments/{appointmentId}/reschedule/{newScheduleId}
    public async Task<Appointment> ReceptionistRescheduleAsync(Guid appointmentId, Guid newScheduleId, Guid receptionistId, CancellationToken ct = default)
    {
        var appointment = await context.Appointments
            .Include(a => a.Schedule)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new InvalidOperationException("A cancelled or completed appointment cannot be rescheduled.");

        if (appointment.ScheduleId == newScheduleId)
            throw new InvalidOperationException("New schedule must be different from the current one.");

        using var _ = await scheduleLock.AcquirePairAsync(appointment.ScheduleId, newScheduleId, ct);

        var newSchedule = await context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == newScheduleId, ct)
            ?? throw new KeyNotFoundException("New schedule not found.");

        if (newSchedule.DoctorId != appointment.DoctorId)
            throw new InvalidOperationException("New schedule does not belong to the appointment's doctor.");

        if (newSchedule.IsBooked)
            throw new InvalidOperationException("New schedule slot is already booked.");

        if (newSchedule.IsBlocked)
            throw new InvalidOperationException("New schedule slot is blocked.");

        appointment.Schedule.IsBooked = false;
        appointment.Schedule.UpdatedAt = DateTime.UtcNow;
        appointment.ScheduleId = newScheduleId;
        appointment.ReceptionistId = receptionistId;
        appointment.UpdatedAt = DateTime.UtcNow;
        newSchedule.IsBooked = true;
        newSchedule.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.PatientId,
            UserType = UserRole.Patient,
            Subject = "Appointment Rescheduled",
            Content = $"Your appointment with Dr. {appointment.Doctor.LastName} has been rescheduled to {newSchedule.Date:MMMM d, yyyy} at {newSchedule.StartTime} by the receptionist.",
            Type = NotificationType.AppointmentRescheduledByReceptionist
        }, ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "Appointment Rescheduled by Receptionist",
            Content = $"The appointment for {appointment.Patient.FirstName} {appointment.Patient.LastName} has been rescheduled to {newSchedule.Date:MMMM d, yyyy} at {newSchedule.StartTime}.",
            Type = NotificationType.AppointmentRescheduledByReceptionist
        }, ct);

        return appointment;
    }

    // PUT /api/receptionist/appointments/{appointmentId}/arrived
    public async Task<Appointment> MarkAsArrivedAsync(Guid appointmentId, Guid receptionistId, CancellationToken ct = default)
    {
        using var _ = await appointmentLock.AcquireAsync(appointmentId, ct);

        var appointment = await context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed appointments can be marked as arrived.");

        appointment.Status = AppointmentStatus.Arrived;
        appointment.ReceptionistId = receptionistId;
        appointment.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "Patient Arrived",
            Content = $"{appointment.Patient.FirstName} {appointment.Patient.LastName} has arrived for their appointment.",
            Type = NotificationType.AppointmentArrived
        }, ct);

        return appointment;
    }

    // PUT /api/receptionist/appointments/{appointmentId}/complete
    public async Task<Appointment> ReceptionistCompleteAsync(Guid appointmentId, Guid receptionistId, CancellationToken ct = default)
    {
        using var _ = await appointmentLock.AcquireAsync(appointmentId, ct);

        var appointment = await context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status is not (AppointmentStatus.Confirmed or AppointmentStatus.Arrived))
            throw new InvalidOperationException("Only confirmed or arrived appointments can be marked as completed.");

        appointment.Status = AppointmentStatus.Completed;
        appointment.ReceptionistId = receptionistId;
        appointment.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.PatientId,
            UserType = UserRole.Patient,
            Subject = "Appointment Completed",
            Content = $"Your appointment with Dr. {appointment.Doctor.LastName} has been marked as completed.",
            Type = NotificationType.AppointmentCompleted
        }, ct);

        await notificationRepo.CreateAsync(new Notification
        {
            UserId = appointment.DoctorId,
            UserType = UserRole.Doctor,
            Subject = "Appointment Completed",
            Content = $"The appointment for {appointment.Patient.FirstName} {appointment.Patient.LastName} has been marked as completed.",
            Type = NotificationType.AppointmentCompleted
        }, ct);

        return appointment;
    }

    public async Task<List<Appointment>> GetExpiryAppointmentsAsync(CancellationToken ct = default)
    {
        var appointment = await context.Appointments
            .Include(a => a.Schedule)
            .Where(a => (
                a.Schedule.Date < DateOnly.FromDateTime(DateTime.Today) &&
                (a.Status == AppointmentStatus.Pending ||
                 a.Status == AppointmentStatus.Confirmed ||
                 a.Status == AppointmentStatus.Arrived)
            ))
            .ToListAsync<Appointment>(ct);

        return appointment;
    }

    public async Task ConvertToExpiredAsync(List<Appointment> expiryAppointments, CancellationToken ct = default)
    {
        foreach (var appointment in expiryAppointments)
        {
            appointment.Status = AppointmentStatus.Expired;
            appointment.Notes = "Appointment has expired.";
            appointment.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task NotifyReceptionistsAsync(string subject, string content, NotificationType type, CancellationToken ct)
    {
        var receptionistIds = await context.Receptionists
            .Where(r => !r.Deactivated)
            .Select(r => r.ReceptionistId)
            .ToListAsync(ct);

        foreach (var receptionistId in receptionistIds)
        {
            await notificationRepo.CreateAsync(new Notification
            {
                UserId = receptionistId,
                UserType = UserRole.Receptionist,
                Subject = subject,
                Content = content,
                Type = type
            }, ct);
        }
    }

    private static bool IsValidTransition(AppointmentStatus from, AppointmentStatus to) =>
        from switch
        {
            AppointmentStatus.Pending => to is AppointmentStatus.Confirmed or AppointmentStatus.Cancelled,
            AppointmentStatus.Confirmed => to is AppointmentStatus.Completed or AppointmentStatus.Cancelled,
            AppointmentStatus.Arrived => to is AppointmentStatus.Completed or AppointmentStatus.Cancelled,
            _ => false
        };

    private static AppointmentSummaryResponse MapToSummary(Appointment a) =>
        new()
        {
            AppointmentId = a.AppointmentId,
            Status = a.Status,
            Reason = a.Reason,
            CancelledBy = a.CancelledBy,
            CancellationNote = a.CancellationNote,
            CreatedAt = a.CreatedAt,
            Patient = a.Patient is null ? null : new PatientSummaryResponse
            {
                PatientId = a.Patient.PatientId,
                FirstName = a.Patient.FirstName,
                MiddleName = a.Patient.MiddleName,
                LastName = a.Patient.LastName,
                Email = a.Patient.Email,
                Phone = a.Patient.Phone,
                Deactivated = a.Patient.Deactivated
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
