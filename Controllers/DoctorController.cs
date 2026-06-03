using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
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
[Route("api/doctor")]
[Authorize(Roles = "Doctor")]
public class DoctorController(
    IDoctorService doctorService,
    IScheduleService scheduleService,
    IAppointmentService appointmentService,
    ISpecializationService specializationService
) : ControllerBase
{
    // GET /api/doctor/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await doctorService.GetProfileAsync(User.GetUserId(), ct);
        return Ok(ResponseWrapper<DoctorProfileResponse>.On(
            result,
            "Profile retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/doctor/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateDoctorRequest request,
        CancellationToken ct
    )
    {
        var result = await doctorService.UpdateProfileAsync(User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<DoctorProfileResponse>.On(
            result,
            "Profile updated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/doctor/profile/password
    [HttpPut("profile/password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct
    )
    {
        await doctorService.ChangePasswordAsync(User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Password changed successfully.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/doctor/schedules
    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] CreateScheduleRequest request,
        CancellationToken ct
    )
    {
        var result = await scheduleService.CreateAsync(User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<ScheduleResponse>.On(
            result,
            "Schedule created.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/doctor/schedules/{scheduleId}
    [HttpPut("schedules/{scheduleId:guid}")]
    public async Task<IActionResult> UpdateSchedule(
        Guid scheduleId,
        [FromBody] UpdateScheduleRequest request,
        CancellationToken ct
    )
    {
        var result = await scheduleService.UpdateAsync(User.GetUserId(), scheduleId, request, ct);
        return Ok(ResponseWrapper<ScheduleResponse>.On(
            result,
            "Schedule updated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/doctor/schedules/{scheduleId}/toggle-block
    [HttpPut("schedules/{scheduleId:guid}/toggle-block")]
    public async Task<IActionResult> ToggleBlock(
        Guid scheduleId,
        CancellationToken ct
    )
    {
        var result = await scheduleService.ToggleBlockAsync(User.GetUserId(), scheduleId, ct);
        return Ok(ResponseWrapper<ScheduleResponse>.On(
            result,
            "Schedule block toggled.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // DELETE /api/doctor/schedules/{scheduleId}
    [HttpDelete("schedules/{scheduleId:guid}")]
    public async Task<IActionResult> DeleteSchedule(
        Guid scheduleId,
        CancellationToken ct
    )
    {
        var result = await scheduleService.DeleteAsync(User.GetUserId(), scheduleId, ct);
        return Ok(ResponseWrapper<ScheduleResponse>.On(
            result,
            "Schedule deleted.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/doctor/schedules
    [HttpGet("schedules")]
    public async Task<IActionResult> GetSchedules(CancellationToken ct)
    {
        var result = await scheduleService.GetByDoctorAsync(User.GetUserId(), ct);
        return Ok(ResponseWrapper<List<ScheduleResponse>>.On(
            result,
            "Schedules retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/doctor/schedules/{scheduleId}
    [HttpGet("schedules/{scheduleId:guid}")]
    public async Task<IActionResult> GetSchedule(
        Guid scheduleId,
        CancellationToken ct
    )
    {
        var result = await scheduleService.GetByIdAsync(scheduleId, ct);
        return Ok(ResponseWrapper<ScheduleResponse>.On(
            result,
            "Schedule retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/doctor/appointments
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] AppointmentFilterRequest request,
        CancellationToken ct
    )
    {
        var result = await appointmentService.GetDoctorAppointmentsAsync(User.GetUserId(), request, ct);
        return Ok(result.ToWrapper("Appointments retrieved."));
    }

    // PUT /api/doctor/appointments/{appointmentId}
    [HttpPut("appointments/{appointmentId:guid}")]
    public async Task<IActionResult> UpdateAppointment(
        Guid appointmentId,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken ct
    )
    {
        var result = await appointmentService.UpdateAsync(appointmentId, User.GetUserId(), request, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment updated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/doctor/appointments/{appointmentId}/confirm
    [HttpPut("appointments/{appointmentId:guid}/confirm")]
    public async Task<IActionResult> ConfirmAppointment(
        Guid appointmentId,
        CancellationToken ct
    )
    {
        var result = await appointmentService.UpdateAsync(
            appointmentId,
            User.GetUserId(),
            new UpdateAppointmentRequest { Status = AppointmentStatus.Confirmed },
            ct);

        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment confirmed.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/doctor/appointments/{appointmentId}/complete
    [HttpPut("appointments/{appointmentId:guid}/complete")]
    public async Task<IActionResult> CompleteAppointment(
        Guid appointmentId,
        [FromBody] UpdateAppointmentRequest request,
        CancellationToken ct
    )
    {
        request.Status = AppointmentStatus.Completed;
        var result = await appointmentService.UpdateAsync(appointmentId, User.GetUserId(), request, ct);

        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment completed.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/doctor/appointments/{appointmentId}
    [HttpGet("appointments/{appointmentId:guid}")]
    public async Task<IActionResult> GetAppointment(
        Guid appointmentId,
        CancellationToken ct
    )
    {
        var result = await appointmentService.GetByIdAsync(appointmentId, ct);

        if (result.Doctor.DoctorId != User.GetUserId())
            return Forbid();

        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/doctor/specializations
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
