using BookingSystem.Dtos.Auth;
using BookingSystem.Enums;
using BookingSystem.Services.Interfaces;
using BookingSystem.Wrapper;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // POST /api/auth/patient/login
    [HttpPost("patient/login")]
    public async Task<IActionResult> PatientLogin(
        [FromBody] DefaultLoginRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.PatientLoginAsync(request, ct);
        return Ok(ResponseWrapper<AuthResponse>.On(
            result,
            "Login successful.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/auth/doctor/login
    [HttpPost("doctor/login")]
    public async Task<IActionResult> DoctorLogin(
        [FromBody] DefaultLoginRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.DoctorLoginAsync(request, ct);
        return Ok(ResponseWrapper<AuthResponse>.On(
            result,
            "Login successful.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/auth/admin/login
    [HttpPost("admin/login")]
    public async Task<IActionResult> AdminLogin(
        [FromBody] AdminLoginRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.AdminLoginAsync(request, ct);
        return Ok(ResponseWrapper<AuthResponse>.On(
            result,
            "Login successful.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/auth/receptionist/login
    [HttpPost("receptionist/login")]
    public async Task<IActionResult> ReceptionistLogin(
        [FromBody] DefaultLoginRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.ReceptionistLoginAsync(request, ct);
        return Ok(ResponseWrapper<AuthResponse>.On(
            result,
            "Login successful.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.RefreshAsync(request.RefreshToken, ct);
        return Ok(ResponseWrapper<AuthResponse>.On(
            result,
            "Token refreshed.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct
    )
    {
        await authService.RevokeAsync(request.RefreshToken, ct);
        return Ok(ResponseWrapper<object>.On(
            null,
            "Logged out successfully.",
            StatusType.Success,
            StatusCodes.Status200OK
        ));
    }
}
