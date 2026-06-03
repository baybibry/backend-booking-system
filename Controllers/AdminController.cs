using BookingSystem.Dtos.Admin;
using BookingSystem.Dtos.Appointment;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Receptionist;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Enums;
using BookingSystem.Services.Interfaces;
using BookingSystem.Wrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    IAdminService adminService,
    IPatientService patientService,
    IDoctorService doctorService,
    ISpecializationService specializationService,
    IAppointmentService appointmentService,
    IReceptionistService receptionistService
) : ControllerBase
{
    // POST /api/admin/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAdmin(
        [FromBody] AdminRegistrationRequest request,
        CancellationToken ct
    )
    {
        var result = await adminService.RegisterAsync(request, ct);
        return Ok(ResponseWrapper<AdminResponse>.On(
            result,
            "Admin registered.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/admin/patients
    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatients(
        [FromQuery] PatientSearchRequest request,
        CancellationToken ct
    )
    {
        var result = await patientService.GetAllAsync(request, ct);
        return Ok(result.ToWrapper("Patients retrieved."));
    }

    // PUT /api/admin/patients/{patientId}/deactivate
    [HttpPut("patients/{patientId:guid}/deactivate")]
    public async Task<IActionResult> DeactivatePatient(
        Guid patientId,
        CancellationToken ct
    )
    {
        await patientService.DeactivateAsync(patientId, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Patient deactivated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/admin/patients/{patientId}/reactivate
    [HttpPut("patients/{patientId:guid}/reactivate")]
    public async Task<IActionResult> ReactivatePatient(
        Guid patientId,
        CancellationToken ct
    )
    {
        await patientService.ReactivateAsync(patientId, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Patient reactivated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/admin/doctors
    [HttpPost("doctors")]
    public async Task<IActionResult> RegisterDoctor(
        [FromBody] DoctorRegistrationRequest request,
        CancellationToken ct
    )
    {
        var result = await doctorService.RegisterAsync(request, ct);
        return Ok(ResponseWrapper<DoctorProfileResponse>.On(
            result,
            "Doctor registered.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/admin/doctors
    [HttpGet("doctors")]
    public async Task<IActionResult> GetAllDoctors(
        [FromQuery] PaginationRequest request,
        CancellationToken ct
    )
    {
        var result = await doctorService.GetAllAsync(request, ct);
        return Ok(result.ToWrapper("Doctors retrieved."));
    }

    // PUT /api/admin/doctors/{doctorId}/deactivate
    [HttpPut("doctors/{doctorId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateDoctor(
        Guid doctorId,
        CancellationToken ct
    )
    {
        await doctorService.DeactivateAsync(doctorId, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Doctor deactivated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/admin/doctors/{doctorId}/reactivate
    [HttpPut("doctors/{doctorId:guid}/reactivate")]
    public async Task<IActionResult> ReactivateDoctor(
        Guid doctorId,
        CancellationToken ct
    )
    {
        await doctorService.ReactivateAsync(doctorId, ct);
        return Ok(ResponseWrapper<string>.On(
            null,
            "Doctor reactivated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/admin/specializations
    [HttpPost("specializations")]
    public async Task<IActionResult> CreateSpecialization(
        [FromBody] CreateSpecializationRequest request,
        CancellationToken ct
    )
    {
        var result = await specializationService.CreateAsync(request, ct);
        return Ok(ResponseWrapper<SpecializationResponse>.On(
            result,
            "Specialization created.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/admin/specializations
    [HttpGet("specializations")]
    [Authorize(Roles = "Admin,Patient,Doctor,Receptionist")]
    public async Task<IActionResult> GetAllSpecializations(CancellationToken ct)
    {
        var result = await specializationService.GetAllAsync(ct);
        return Ok(ResponseWrapper<List<SpecializationResponse>>.On(
            result,
            "Specializations retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // GET /api/admin/specializations/{id}
    [HttpGet("specializations/{id:guid}")]
    public async Task<IActionResult> GetSpecialization(
        Guid id,
        CancellationToken ct
    )
    {
        var result = await specializationService.GetByIdAsync(id, ct);
        return Ok(ResponseWrapper<SpecializationResponse>.On(
            result,
            "Specialization retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // PUT /api/admin/specializations/{id}
    [HttpPut("specializations/{id:guid}")]
    public async Task<IActionResult> UpdateSpecialization(
        Guid id,
        [FromBody] UpdateSpecializationRequest request,
        CancellationToken ct
    )
    {
        var result = await specializationService.UpdateAsync(id, request, ct);
        return Ok(ResponseWrapper<SpecializationResponse>.On(
            result,
            "Specialization updated.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // DELETE /api/admin/specializations/{id}
    [HttpDelete("specializations/{id:guid}")]
    public async Task<IActionResult> DeleteSpecialization(
        Guid id,
        CancellationToken ct
    )
    {
        var result = await specializationService.DeleteAsync(id, ct);
        return Ok(ResponseWrapper<SpecializationResponse>.On(
            result,
            "Specialization deleted.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/admin/receptionists
    [HttpPost("receptionists")]
    public async Task<IActionResult> RegisterReceptionist(
        [FromBody] ReceptionistRegistrationRequest request,
        CancellationToken ct
    )
    {
        var result = await receptionistService.RegisterAsync(request, ct);
        return Ok(ResponseWrapper<ReceptionistProfileResponse>.On(
            result, "Receptionist registered.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/admin/receptionists
    [HttpGet("receptionists")]
    public async Task<IActionResult> GetReceptionists(
        [FromQuery] PaginationRequest request,
        CancellationToken ct
    )
    {
        var result = await receptionistService.GetAllAsync(request, ct);
        return Ok(result.ToWrapper("Receptionists retrieved."));
    }

    // PUT /api/admin/receptionists/{receptionistId}/deactivate
    [HttpPut("receptionists/{receptionistId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateReceptionist(Guid receptionistId, CancellationToken ct)
    {
        await receptionistService.DeactivateAsync(receptionistId, ct);
        return Ok(ResponseWrapper<string>.On(
            null, "Receptionist deactivated.", StatusType.Success, StatusCodes.Status200OK));
    }

    // PUT /api/admin/receptionists/{receptionistId}/reactivate
    [HttpPut("receptionists/{receptionistId:guid}/reactivate")]
    public async Task<IActionResult> ReactivateReceptionist(Guid receptionistId, CancellationToken ct)
    {
        await receptionistService.ReactivateAsync(receptionistId, ct);
        return Ok(ResponseWrapper<string>.On(
            null, "Receptionist reactivated.", StatusType.Success, StatusCodes.Status200OK));
    }

    // GET /api/admin/appointments
    [HttpGet("appointments")]
    public async Task<IActionResult> GetAllAppointments(
        [FromQuery] AppointmentFilterRequest request,
        CancellationToken ct
    )
    {
        var result = await appointmentService.GetAllAsync(request, ct);
        return Ok(result.ToWrapper("Appointments retrieved."));
    }

    // GET /api/admin/appointments/{appointmentId}
    [HttpGet("appointments/{appointmentId:guid}")]
    public async Task<IActionResult> GetAppointment(
        Guid appointmentId,
        CancellationToken ct
    )
    {
        var result = await appointmentService.GetByIdAsync(appointmentId, ct);
        return Ok(ResponseWrapper<AppointmentResponse>.On(
            result,
            "Appointment retrieved.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }
}
