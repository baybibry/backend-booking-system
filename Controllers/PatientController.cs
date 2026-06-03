using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Auth;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Patient;
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
[Route("api/patient")]
public class PatientController(
    IPatientService patientService,
    IDoctorService doctorService,
    IScheduleService scheduleService,
    IAppointmentService appointmentService,
    ISpecializationService specializationService
) : ControllerBase
{
    // POST /api/patient/registration
    [HttpPost("registration")]
    public async Task<IActionResult> Register(
        [FromBody] PatientRegistrationRequest request,
        CancellationToken ct
    )
    {
        await patientService.RegisterAsync(request, ct);
        return Ok(ResponseWrapper<string>.On(
            "Successfully register",
            "Registration successful.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/patient/profile
    [Authorize(Roles = "Patient")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await patientService.GetProfileAsync(User.GetUserId(), ct);
        return Ok(ResponseWrapper<PatientProfileResponse>.On(
            result,
            "Profile retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/patient/profile
    [Authorize(Roles = "Patient")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdatePatientRequest request,
        CancellationToken ct
    )
    {
        var result = await patientService.UpdateProfileAsync(User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<PatientProfileResponse>.On(
            result,
            "Profile updated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/patient/profile/password
    [Authorize(Roles = "Patient")]
    [HttpPut("profile/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct
    )
    {
        await patientService.ChangePasswordAsync(User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Password changed successfully.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/patient/doctors
    [Authorize(Roles = "Patient")]
    [HttpGet("doctors")]
    public async Task<IActionResult> SearchDoctors(
        [FromQuery] DoctorSearchRequest request,
        CancellationToken ct
    )
    {
        var result = await doctorService.SearchAsync(request, ct);
        return Ok(result.ToWrapper("Doctors retrieved."));
    }

    // GET /api/patient/doctors/{doctorId}/schedules
    [Authorize(Roles = "Patient")]
    [HttpGet("doctors/{doctorId:guid}/schedules")]
    public async Task<IActionResult> GetDoctorSchedules(
        Guid doctorId,
        CancellationToken ct
    )
    {
        var result = await scheduleService.GetAvailableByDoctorAsync(doctorId, ct);
        return Ok(ResponseWrapper<List<ScheduleResponse>>.On(
            result,
            "Schedules retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/patient/appointments/{doctorId}
    [Authorize(Roles = "Patient")]
    [HttpPost("appointments/{doctorId:guid}")]
    public async Task<IActionResult> BookAppointment(
        Guid doctorId,
        [FromBody] BookAppointmentRequest request,
        CancellationToken ct
    )
    {
        var result = await appointmentService.BookAsync(User.GetUserId(), doctorId, request, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment booked.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/patient/appointments/cancel
    [Authorize(Roles = "Patient")]
    [HttpPut("appointments/cancel")]
    public async Task<IActionResult> CancelAppointment(
        [FromBody] CancelAppointmentRequest request,
        CancellationToken ct
    )
    {
        var result = await appointmentService.CancelAsync(User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment cancelled.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/patient/appointments/reschedule
    [Authorize(Roles = "Patient")]
    [HttpPut("appointments/reschedule")]
    public async Task<IActionResult> RescheduleAppointment(
        [FromBody] RescheduleAppointmentRequest request,
        CancellationToken ct
    )
    {
        var result = await appointmentService.RescheduleAsync(User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment rescheduled.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/patient/appointments
    [Authorize(Roles = "Patient")]
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointmentHistory(
        [FromQuery] AppointmentFilterRequest request,
        CancellationToken ct
    )
    {
        var result = await appointmentService.GetPatientHistoryAsync(User.GetUserId(), request, ct);
        return Ok(result.ToWrapper("Appointments retrieved."));
    }

    // GET /api/patient/appointments/{appointmentId}
    [Authorize(Roles = "Patient")]
    [HttpGet("appointments/{appointmentId:guid}")]
    public async Task<IActionResult> GetAppointment(
        Guid appointmentId,
        CancellationToken ct
    )
    {
        var result = await appointmentService.GetByIdAsync(appointmentId, ct);

        if (result.Patient.PatientId != User.GetUserId())
            return Forbid();

        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/patient/specializations
    [Authorize(Roles = "Patient")]
    [HttpGet("specializations")]
    public async Task<IActionResult> GetSpecializations(CancellationToken ct)
    {
        var result = await specializationService.GetAllAsync(ct);
        return Ok(ResponseWrapper<List<SpecializationResponse>>.On(
            result,
            "Specializations retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }
}
