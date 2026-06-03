using BookingSystem.Dtos.Common;
using BookingSystem.Dtos.Doctor;
using BookingSystem.Dtos.Specialization;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;

namespace BookingSystem.Services.Implementations;

public class DoctorService(
    IDoctorRepository doctorRepo,
    ISpecializationRepository specializationRepo
) : IDoctorService
{
    public async Task<DoctorProfileResponse> RegisterAsync(
        DoctorRegistrationRequest request,
        CancellationToken ct = default
    )
    {
        if (await doctorRepo.EmailExistsAsync(request.Email, ct))
            throw new InvalidOperationException("Email is already in use.");

        if (await doctorRepo.LicenseNoExistsAsync(request.LicenseNo, ct))
            throw new InvalidOperationException("License number is already in use.");

        var specialization = await specializationRepo.GetByIdAsync(request.SpecializationId, ct)
            ?? throw new KeyNotFoundException("Specialization not found.");

        var doctor = new Doctor
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            LicenseNo = request.LicenseNo,
            SpecializationId = request.SpecializationId,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await doctorRepo.CreateAsync(doctor, ct);
        created.Specialization = specialization;
        return MapToProfile(created);
    }

    public async Task<DoctorProfileResponse> GetProfileAsync(
        Guid doctorId,
        CancellationToken ct = default
    )
    {
        var doctor = await doctorRepo.GetByIdAsync(doctorId, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        return MapToProfile(doctor);
    }

    public async Task<DoctorProfileResponse> UpdateProfileAsync(
        Guid doctorId,
        UpdateDoctorRequest request,
        CancellationToken ct = default
    )
    {
        var updated = await doctorRepo.UpdateAsync(doctorId, request, ct);
        return MapToProfile(updated);
    }

    public async Task ChangePasswordAsync(
        Guid doctorId,
        ChangePasswordRequest request,
        CancellationToken ct = default
    )
    {
        var doctor = await doctorRepo.GetByIdAsync(doctorId, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, doctor.Password))
            throw new InvalidCastException("Current password is incorrect.");

        var hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await doctorRepo.UpdatePasswordAsync(doctorId, hashed, ct);
    }

    public async Task<PagedResponse<DoctorSummaryResponse>> SearchAsync(
        DoctorSearchRequest request,
        CancellationToken ct = default
    ) => await doctorRepo.SearchAsync(request, ct);

    public async Task<PagedResponse<DoctorSummaryResponse>> GetAllAsync(
        PaginationRequest request,
        CancellationToken ct = default
    ) => await doctorRepo.GetAllAsync(request, ct);

    public async Task DeactivateAsync(
        Guid doctorId,
        CancellationToken ct = default
    )
    {
        var doctor = await doctorRepo.GetByIdAsync(doctorId, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        if (doctor.Deactivated)
            throw new InvalidOperationException("Doctor is already deactivated.");

        await doctorRepo.DeactivateAsync(doctorId, ct);
    }

    public async Task ReactivateAsync(
        Guid doctorId,
        CancellationToken ct = default
    )
    {
        var doctor = await doctorRepo.GetByIdAsync(doctorId, ct)
            ?? throw new KeyNotFoundException("Doctor not found.");

        if (!doctor.Deactivated)
            throw new InvalidOperationException("Doctor is already active.");

        await doctorRepo.ReactivateAsync(doctorId, ct);
    }

    private static DoctorProfileResponse MapToProfile(Doctor d) => new()
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
}
