using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Patient;
using BookingSystem.Dtos.Receptionist;
using BookingSystem.Dtos.Schedule;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;

namespace BookingSystem.Services.Implementations;

public class ReceptionistService(
    IReceptionistRepository receptionistRepo,
    IDoctorRepository doctorRepo,
    IPatientRepository patientRepo,
    IAppointmentRepository appointmentRepo,
    IScheduleRepository scheduleRepo
) : IReceptionistService
{

    public async Task<ReceptionistProfileResponse> RegisterAsync(
        ReceptionistRegistrationRequest request,
        CancellationToken ct = default
    )
    {
        if (await receptionistRepo.EmailExistsAsync(request.Email, ct))
            throw new InvalidOperationException("Email is already in use.");

        if (await receptionistRepo.EmployeeNoExistsAsync(request.EmployeeNo, ct))
            throw new InvalidOperationException("Employee number is already in use.");

        var receptionist = new Receptionist
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            EmployeeNo = request.EmployeeNo,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await receptionistRepo.CreateAsync(receptionist, ct);
        return MapToProfile(created);
    }

    public async Task<PagedResponse<ReceptionistSummaryResponse>> GetAllAsync(
        PaginationRequest request,
        CancellationToken ct = default
    ) => await receptionistRepo.GetAllAsync(request, ct);

    public async Task DeactivateAsync(Guid receptionistId, CancellationToken ct = default)
    {
        _ = await receptionistRepo.DeactivateAsync(receptionistId, ct);
    }

    public async Task ReactivateAsync(Guid receptionistId, CancellationToken ct = default)
    {
        _ = await receptionistRepo.ReactivateAsync(receptionistId, ct);
    }


    public async Task<ReceptionistProfileResponse> GetProfileAsync(
        Guid receptionistId,
        CancellationToken ct = default
    )
    {
        var receptionist = await receptionistRepo.GetByIdAsync(receptionistId, ct)
            ?? throw new KeyNotFoundException("Receptionist not found.");

        return MapToProfile(receptionist);
    }

    public async Task<ReceptionistProfileResponse> UpdateProfileAsync(
        Guid receptionistId,
        UpdateReceptionistRequest request,
        CancellationToken ct = default
    )
    {
        var updated = await receptionistRepo.UpdateAsync(receptionistId, request, ct);
        return MapToProfile(updated);
    }

    public async Task ChangePasswordAsync(
        Guid receptionistId,
        ChangePasswordRequest request,
        CancellationToken ct = default
    )
    {
        var receptionist = await receptionistRepo.GetByIdAsync(receptionistId, ct)
            ?? throw new KeyNotFoundException("Receptionist not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, receptionist.Password))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        await receptionistRepo.UpdatePasswordAsync(receptionistId, BCrypt.Net.BCrypt.HashPassword(request.NewPassword), ct);
    }


    public async Task<PagedResponse<DoctorSummaryResponse>> GetDoctorsAsync(
        DoctorSearchRequest request,
        CancellationToken ct = default
    ) => await doctorRepo.SearchAsync(request, ct);

    public async Task<DoctorProfileResponse> GetDoctorAsync(
        Guid doctorId,
        CancellationToken ct = default
    )
    {
        var doctor = await doctorRepo.GetByIdAsync(doctorId, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        return MapDoctorToProfile(doctor);
    }

    public async Task<List<ScheduleResponse>> GetDoctorSchedulesAsync(
        Guid doctorId,
        CancellationToken ct = default
    )
    {
        var schedules = await scheduleRepo.GetByDoctorAsync(doctorId, ct);
        return schedules.Select(MapScheduleToResponse).ToList();
    }

    public async Task<List<ScheduleWithDoctorResponse>> GetAllNonExpiredSchedulesAsync(
        CancellationToken ct = default
    )
    {
        var schedules = await scheduleRepo.GetAllNonExpiredAsync(ct);
        return schedules.Select(s => new ScheduleWithDoctorResponse
        {
            ScheduleId = s.ScheduleId,
            Date = s.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsBooked = s.IsBooked,
            IsBlocked = s.IsBlocked,
            Doctor = new DoctorSummaryResponse
            {
                DoctorId = s.Doctor.DoctorId,
                FirstName = s.Doctor.FirstName,
                MiddleName = s.Doctor.MiddleName,
                LastName = s.Doctor.LastName,
                Email = s.Doctor.Email,
                Phone = s.Doctor.Phone,
                Deactivated = s.Doctor.Deactivated
            }
        }).ToList();
    }


    public async Task<PagedResponse<PatientSummaryResponse>> GetPatientsAsync(
        PatientSearchRequest request,
        CancellationToken ct = default
    ) => await patientRepo.GetAllAsync(request, ct);

    public async Task<PatientProfileResponse> GetPatientAsync(
        Guid patientId,
        CancellationToken ct = default
    )
    {
        var patient = await patientRepo.GetByIdAsync(patientId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        return MapPatientToProfile(patient);
    }

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetPatientAppointmentsAsync(
        Guid patientId,
        AppointmentFilterRequest request,
        CancellationToken ct = default
    ) => await appointmentRepo.GetPatientHistoryAsync(patientId, request, ct);


    public async Task<AppointmentResponse> AcceptAppointmentAsync(
        Guid receptionistId,
        Guid appointmentId,
        CancellationToken ct = default
    )
    {
        var updated = await appointmentRepo.ReceptionistConfirmAsync(appointmentId, receptionistId, ct);
        var full = await appointmentRepo.GetByIdAsync(updated.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapAppointmentToResponse(full);
    }

    public async Task<AppointmentResponse> CancelAppointmentAsync(
        Guid receptionistId,
        Guid appointmentId,
        string? cancellationNote,
        CancellationToken ct = default
    )
    {
        var updated = await appointmentRepo.ReceptionistCancelAsync(appointmentId, receptionistId, cancellationNote, ct);
        var full = await appointmentRepo.GetByIdAsync(updated.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapAppointmentToResponse(full);
    }

    public async Task<AppointmentResponse> RescheduleAppointmentAsync(
        Guid receptionistId,
        Guid appointmentId,
        Guid newScheduleId,
        CancellationToken ct = default
    )
    {
        var updated = await appointmentRepo.ReceptionistRescheduleAsync(appointmentId, newScheduleId, receptionistId, ct);
        var full = await appointmentRepo.GetByIdAsync(updated.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapAppointmentToResponse(full);
    }

    public async Task<AppointmentResponse> MarkArrivedAsync(
        Guid receptionistId,
        Guid appointmentId,
        CancellationToken ct = default
    )
    {
        var updated = await appointmentRepo.MarkAsArrivedAsync(appointmentId, receptionistId, ct);
        var full = await appointmentRepo.GetByIdAsync(updated.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapAppointmentToResponse(full);
    }

    public async Task<AppointmentResponse> CompleteAppointmentAsync(
        Guid receptionistId,
        Guid appointmentId,
        CancellationToken ct = default
    )
    {
        var updated = await appointmentRepo.ReceptionistCompleteAsync(appointmentId, receptionistId, ct);
        var full = await appointmentRepo.GetByIdAsync(updated.AppointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapAppointmentToResponse(full);
    }

    public async Task<PagedResponse<AppointmentSummaryResponse>> GetAppointmentsAsync(
        AppointmentFilterRequest request,
        CancellationToken ct = default
    ) => await appointmentRepo.GetAllAsync(request, ct);

    public async Task<AppointmentResponse> GetAppointmentAsync(
        Guid appointmentId,
        CancellationToken ct = default
    )
    {
        var appointment = await appointmentRepo.GetByIdAsync(appointmentId, ct)
            ?? throw new KeyNotFoundException("Appointment not found.");

        return MapAppointmentToResponse(appointment);
    }


    public async Task<ScheduleResponse> CreateScheduleForDoctorAsync(
        ReceptionistCreateScheduleRequest request,
        CancellationToken ct = default
    )
    {
        var doctor = await doctorRepo.GetByIdAsync(request.DoctorId, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        if (doctor.Deactivated)
            throw new InvalidOperationException("Cannot create a schedule for a deactivated doctor.");

        if (await scheduleRepo.HasOverlapAsync(request.DoctorId, request.Date, request.StartTime, request.EndTime, ct: ct))
            throw new InvalidOperationException("A schedule slot already exists that overlaps with the requested date and time.");

        var schedule = new Schedule
        {
            DoctorId = request.DoctorId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        var created = await scheduleRepo.CreateAsync(schedule, ct);
        return MapScheduleToResponse(created);
    }

    public async Task<ScheduleResponse> ToggleBlockScheduleAsync(
        Guid scheduleId,
        CancellationToken ct = default
    )
    {
        var schedule = await scheduleRepo.GetByIdAsync(scheduleId, ct)
            ?? throw new KeyNotFoundException("Schedule not found.");

        var updated = await scheduleRepo.ToggleBlockAsync(schedule.ScheduleId, ct);
        return MapScheduleToResponse(updated);
    }


    private static ReceptionistProfileResponse MapToProfile(Receptionist r) => new()
    {
        ReceptionistId = r.ReceptionistId,
        FirstName = r.FirstName,
        MiddleName = r.MiddleName,
        LastName = r.LastName,
        Email = r.Email,
        Phone = r.Phone,
        EmployeeNo = r.EmployeeNo,
        Deactivated = r.Deactivated,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    private static DoctorProfileResponse MapDoctorToProfile(Doctor d) => new()
    {
        DoctorId = d.DoctorId,
        FirstName = d.FirstName,
        MiddleName = d.MiddleName,
        LastName = d.LastName,
        Email = d.Email,
        Phone = d.Phone,
        LicenseNo = d.LicenseNo,
        Specialization = new SpecializationResponse
        {
            SpecializationId = d.Specialization.SpecializationId,
            SpecializationName = d.Specialization.SpecializationName
        }
    };

    private static PatientProfileResponse MapPatientToProfile(Patient p) => new()
    {
        PatientId = p.PatientId,
        FirstName = p.FirstName,
        MiddleName = p.MiddleName,
        LastName = p.LastName,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address
    };

    private static ScheduleResponse MapScheduleToResponse(Schedule s) => new()
    {
        ScheduleId = s.ScheduleId,
        Date = s.Date,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        IsBooked = s.IsBooked,
        IsBlocked = s.IsBlocked
    };

    private static AppointmentResponse MapAppointmentToResponse(Appointment a) => new()
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
