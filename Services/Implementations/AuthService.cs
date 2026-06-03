using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookingSystem.Dtos.Auth;
using BookingSystem.Entities;
using BookingSystem.Enums;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace BookingSystem.Services.Implementations;

public class AuthService(
    IPatientRepository patientRepo,
    IDoctorRepository doctorRepo,
    IAdminRepository adminRepo,
    IReceptionistRepository receptionistRepo,
    IRefreshTokenRepository refreshTokenRepo,
    IConfiguration config
) : IAuthService
{
    public async Task<AuthResponse> PatientLoginAsync(DefaultLoginRequest request, CancellationToken ct = default)
    {
        var patient = await patientRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Patient not found.");

        if (patient.Deactivated)
            throw new UnauthorizedAccessException("Patient is deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, patient.Password))
            throw new UnauthorizedAccessException("Invalid password.");

        return await BuildAuthResponseAsync(patient.PatientId, patient.Email, UserRole.Patient, ct);
    }

    public async Task<AuthResponse> DoctorLoginAsync(DefaultLoginRequest request, CancellationToken ct = default)
    {
        var doctor = await doctorRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Doctor not found.");

        if (doctor.Deactivated)
            throw new UnauthorizedAccessException("Doctor is deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, doctor.Password))
            throw new UnauthorizedAccessException("Invalid password.");

        return await BuildAuthResponseAsync(doctor.DoctorId, doctor.Email, UserRole.Doctor, ct);
    }

    public async Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request, CancellationToken ct = default)
    {
        var admin = await adminRepo.GetByUsernameAsync(request.Username, ct)
            ?? throw new UnauthorizedAccessException("Admin not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.Password))
            throw new UnauthorizedAccessException("Invalid password.");

        return await BuildAuthResponseAsync(admin.AdminId, admin.Username, UserRole.Admin, ct);
    }

    public async Task<AuthResponse> ReceptionistLoginAsync(DefaultLoginRequest request, CancellationToken ct = default)
    {
        var receptionist = await receptionistRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Receptionist not found.");

        if (receptionist.Deactivated)
            throw new UnauthorizedAccessException("Receptionist is deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, receptionist.Password))
            throw new UnauthorizedAccessException("Invalid password.");

        return await BuildAuthResponseAsync(receptionist.ReceptionistId, receptionist.Email, UserRole.Receptionist, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = HashToken(refreshToken.Trim());

        var stored = await refreshTokenRepo.GetByHashAsync(hash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (stored.IsRevoked)
            throw new UnauthorizedAccessException("Refresh token has been revoked.");

        if (stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");

        await refreshTokenRepo.RevokeAsync(stored, ct);

        var identifier = await GetIdentifierAsync(stored.UserId, stored.UserRole, ct);

        return await BuildAuthResponseAsync(stored.UserId, identifier, stored.UserRole, ct);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var hash = HashToken(refreshToken.Trim());

        var stored = await refreshTokenRepo.GetByHashAsync(hash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!stored.IsRevoked)
            await refreshTokenRepo.RevokeAsync(stored, ct);
    }


    private async Task<AuthResponse> BuildAuthResponseAsync(
        Guid userId, string identifier, UserRole role, CancellationToken ct)
    {
        var accessToken = GenerateAccessToken(userId.ToString(), identifier, role);
        var (plainToken, hash) = GenerateRefreshToken();

        var expiryDays = Convert.ToInt32(config["JWT:REFRESH_EXPIRATION_DAYS"] ?? "7");

        await refreshTokenRepo.CreateAsync(new RefreshToken
        {
            UserId = userId,
            UserRole = role,
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        }, ct);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = plainToken,
            Role = role
        };
    }

    private string GenerateAccessToken(string userId, string identifier, UserRole role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, identifier),
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

    private static (string plain, string hash) GenerateRefreshToken()
    {
        var plain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashToken(plain);
        return (plain, hash);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private async Task<string> GetIdentifierAsync(Guid userId, UserRole role, CancellationToken ct) =>
        role switch
        {
            UserRole.Patient => (await patientRepo.GetByIdAsync(userId, ct))?.Email ?? userId.ToString(),
            UserRole.Doctor => (await doctorRepo.GetByIdAsync(userId, ct))?.Email ?? userId.ToString(),
            UserRole.Receptionist => (await receptionistRepo.GetByIdAsync(userId, ct))?.Email ?? userId.ToString(),
            UserRole.Admin => (await adminRepo.GetByIdAsync(userId, ct))?.Username ?? userId.ToString(),
            _ => userId.ToString()
        };
}
