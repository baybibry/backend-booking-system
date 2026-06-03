using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Patient;
using BookingSystem.Dtos.Receptionist;
using BookingSystem.Dtos.Schedule;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Enums;
using BookingSystem.Helper;
using BookingSystem.Services.Interfaces;
using BookingSystem.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/receptionist")]
[Authorize(Roles = "Receptionist")]
public class ReceptionistController(
    IReceptionistService receptionistService,
    ISpecializationService specializationService) : ControllerBase
{
    // GET /api/receptionist/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var id = User.GetUserId();
        var result = await receptionistService.GetProfileAsync(id, ct);
        return Ok(ResponseWrapper<ReceptionistProfileResponse>.On(
            result, "Profile retrieved.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateReceptionistRequest request,
        CancellationToken ct
    )
    {
        var id = User.GetUserId();
        var result = await receptionistService.UpdateProfileAsync(id, request, ct);
        return Ok(ResponseWrapper<ReceptionistProfileResponse>.On(
            result, "Profile updated.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/profile/password
    [HttpPut("profile/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct
    )
    {
        var id = User.GetUserId();
        await receptionistService.ChangePasswordAsync(id, request, ct);
        return Ok(ResponseWrapper<string>.On(
            null, "Password changed.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/receptionist/doctors
    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors(
        [FromQuery] DoctorSearchRequest request,
        CancellationToken ct
    )
    {
        var result = await receptionistService.GetDoctorsAsync(request, ct);
        return Ok(result.ToWrapper("Doctors retrieved."));
    }

    // GET /api/receptionist/doctors/{doctorId}
    [HttpGet("doctors/{doctorId:guid}")]
    public async Task<IActionResult> GetDoctor(Guid doctorId, CancellationToken ct)
    {
        var result = await receptionistService.GetDoctorAsync(doctorId, ct);
        return Ok(ResponseWrapper<DoctorProfileResponse>.On(
            result, "Doctor retrieved.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/receptionist/schedules/all
    [HttpGet("schedules/all")]
    public async Task<IActionResult> GetAllSchedules(CancellationToken ct)
    {
        var result = await receptionistService.GetAllNonExpiredSchedulesAsync(ct);
        return Ok(ResponseWrapper<List<ScheduleWithDoctorResponse>>.On(
            result, "Schedules retrieved.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/receptionist/doctors/{doctorId}/schedules
    [HttpGet("doctors/{doctorId:guid}/schedules")]
    public async Task<IActionResult> GetDoctorSchedules(Guid doctorId, CancellationToken ct)
    {
        var result = await receptionistService.GetDoctorSchedulesAsync(doctorId, ct);
        return Ok(ResponseWrapper<List<ScheduleResponse>>.On(
            result, "Schedules retrieved.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/receptionist/patients
    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients(
        [FromQuery] PatientSearchRequest request,
        CancellationToken ct
    )
    {
        var result = await receptionistService.GetPatientsAsync(request, ct);
        return Ok(result.ToWrapper("Patients retrieved."));
    }

    // GET /api/receptionist/patients/{patientId}
    [HttpGet("patients/{patientId:guid}")]
    public async Task<IActionResult> GetPatient(Guid patientId, CancellationToken ct)
    {
        var result = await receptionistService.GetPatientAsync(patientId, ct);
        return Ok(ResponseWrapper<PatientProfileResponse>.On(
            result, "Patient retrieved.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/receptionist/patients/{patientId}/appointments
    [HttpGet("patients/{patientId:guid}/appointments")]
    public async Task<IActionResult> GetPatientAppointments(
        Guid patientId,
        [FromQuery] AppointmentFilterRequest request,
        CancellationToken ct
    )
    {
        var result = await receptionistService.GetPatientAppointmentsAsync(patientId, request, ct);
        return Ok(result.ToWrapper("Patient appointments retrieved."));
    }

    // GET /api/receptionist/appointments
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] AppointmentFilterRequest request,
        CancellationToken ct
    )
    {
        var result = await receptionistService.GetAppointmentsAsync(request, ct);
        return Ok(result.ToWrapper("Appointments retrieved."));
    }

    // GET /api/receptionist/appointments/{appointmentId}
    [HttpGet("appointments/{appointmentId:guid}")]
    public async Task<IActionResult> GetAppointment(Guid appointmentId, CancellationToken ct)
    {
        var result = await receptionistService.GetAppointmentAsync(appointmentId, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result, "Appointment retrieved.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/appointments/{appointmentId}/accept
    [HttpPut("appointments/{appointmentId:guid}/accept")]
    public async Task<IActionResult> AcceptAppointment(Guid appointmentId, CancellationToken ct)
    {
        var id = User.GetUserId();
        var result = await receptionistService.AcceptAppointmentAsync(id, appointmentId, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result, "Appointment accepted.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/appointments/{appointmentId}/cancel
    [HttpPut("appointments/{appointmentId:guid}/cancel")]
    public async Task<IActionResult> CancelAppointment(
        Guid appointmentId,
        [FromBody] ReceptionistCancelAppointmentRequest? request,
        CancellationToken ct)
    {
        var id = User.GetUserId();
        var result = await receptionistService.CancelAppointmentAsync(id, appointmentId, request?.CancellationNote, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result, "Appointment cancelled.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/appointments/{appointmentId}/reschedule/{newScheduleId}
    [HttpPut("appointments/{appointmentId:guid}/reschedule/{newScheduleId:guid}")]
    public async Task<IActionResult> RescheduleAppointment(
        Guid appointmentId,
        Guid newScheduleId,
        CancellationToken ct
    )
    {
        var id = User.GetUserId();
        var result = await receptionistService.RescheduleAppointmentAsync(id, appointmentId, newScheduleId, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result, "Appointment rescheduled.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/appointments/{appointmentId}/arrived
    [HttpPut("appointments/{appointmentId:guid}/arrived")]
    public async Task<IActionResult> MarkArrived(Guid appointmentId, CancellationToken ct)
    {
        var id = User.GetUserId();
        var result = await receptionistService.MarkArrivedAsync(id, appointmentId, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result, "Patient marked as arrived.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/appointments/{appointmentId}/complete
    [HttpPut("appointments/{appointmentId:guid}/complete")]
    public async Task<IActionResult> CompleteAppointment(Guid appointmentId, CancellationToken ct)
    {
        var id = User.GetUserId();
        var result = await receptionistService.CompleteAppointmentAsync(id, appointmentId, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result, "Appointment completed.", StatusType.Success, StatusCodes.Status200OK));
    }

    // POST /api/receptionist/schedules
    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] ReceptionistCreateScheduleRequest request,
        CancellationToken ct
    )
    {
        var result = await receptionistService.CreateScheduleForDoctorAsync(request, ct);
        return Ok(ResponseWrapper<ScheduleResponse>.On(
            result, "Schedule created.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/receptionist/schedules/{scheduleId}/toggle-block
    [HttpPut("schedules/{scheduleId:guid}/toggle-block")]
    public async Task<IActionResult> ToggleBlockSchedule(Guid scheduleId, CancellationToken ct)
    {
        var result = await receptionistService.ToggleBlockScheduleAsync(scheduleId, ct);
        return Ok(ResponseWrapper<ScheduleResponse>.On(
            result, "Schedule block toggled.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/receptionist/specializations
    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations(CancellationToken ct)
    {
        var result = await specializationService.GetAllAsync(ct);
        return Ok(ResponseWrapper<List<SpecializationResponse>>.On(
            result, "Specializations retrieved.", StatusType.Success, StatusCodes.Status200OK));
    }
}
