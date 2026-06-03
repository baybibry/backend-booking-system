using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookingSystem.Dtos.Auth;
using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Patient;
using BookingSystem.Entities;
using BookingSystem.Enums;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BookingSystem.Services.Implementations;

public class PatientService(
    IPatientRepository patientRepo,
    IConfiguration config
) : IPatientService
{
    public async Task<AuthResponse> RegisterAsync(
        PatientRegistrationRequest request,
        CancellationToken ct = default
    )
    {
        if (await patientRepo.EmailExistsAsync(request.Email, ct))
            throw new InvalidOperationException("Email is already in use.");

        var patient = new Patient
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await patientRepo.CreateAsync(patient, ct);

        return new AuthResponse
        {
            Token = GenerateToken(created.PatientId.ToString(), created.Email, UserRole.Patient),
            Role = UserRole.Patient
        };
    }

    public async Task<PatientProfileResponse> GetProfileAsync(
        Guid patientId,
        CancellationToken ct = default
    )
    {
        var patient = await patientRepo.GetByIdAsync(patientId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        return MapToProfile(patient);
    }

    public async Task<PatientProfileResponse> UpdateProfileAsync(
        Guid patientId,
        UpdatePatientRequest request,
        CancellationToken ct = default
    )
    {
        var updated = await patientRepo.UpdateAsync(patientId, request, ct);
        return MapToProfile(updated);
    }

    public async Task ChangePasswordAsync(
        Guid patientId,
        ChangePasswordRequest request,
        CancellationToken ct = default
    )
    {
        var patient = await patientRepo.GetByIdAsync(patientId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, patient.Password))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        var hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await patientRepo.UpdatePasswordAsync(patientId, hashed, ct);
    }

    public async Task<PagedResponse<PatientSummaryResponse>> GetAllAsync(
        PatientSearchRequest request,
        CancellationToken ct = default
    ) => await patientRepo.GetAllAsync(request, ct);

    public async Task DeactivateAsync(
        Guid patientId,
        CancellationToken ct = default
    )
    {
        var patient = await patientRepo.GetByIdAsync(patientId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (patient.Deactivated)
            throw new InvalidOperationException("Patient is already deactivated.");

        await patientRepo.DeactivateAsync(patientId, ct);
    }

    public async Task ReactivateAsync(
        Guid patientId,
        CancellationToken ct = default
    )
    {
        var patient = await patientRepo.GetByIdAsync(patientId, ct)
            ?? throw new KeyNotFoundException("Patient not found.");

        if (!patient.Deactivated)
            throw new InvalidOperationException("Patient is already active.");

        await patientRepo.ReactivateAsync(patientId, ct);
    }

    private string GenerateToken(string userId, string email, UserRole role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:KEY"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["JWT:ISSUER"],
            audience: config["JWT:AUDIENCE"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(config["JWT:EXPIRATION"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static PatientProfileResponse MapToProfile(Patient p) => new()
    {
        PatientId = p.PatientId,
        FirstName = p.FirstName,
        MiddleName = p.MiddleName,
        LastName = p.LastName,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address
    };
}
