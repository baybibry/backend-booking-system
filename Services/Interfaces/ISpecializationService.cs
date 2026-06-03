using BookingSystem.Dtos.Specialization;

namespace BookingSystem.Services.Interfaces;

public interface ISpecializationService
{
    Task<SpecializationResponse> CreateAsync(
        CreateSpecializationRequest request,
        CancellationToken ct = default
    );

    Task<SpecializationResponse> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    );

    Task<List<SpecializationResponse>> GetAllAsync(
        CancellationToken ct = default
    );

    Task<SpecializationResponse> UpdateAsync(
        Guid id,
        UpdateSpecializationRequest request,
        CancellationToken ct = default
    );

    Task<SpecializationResponse> DeleteAsync(
        Guid id,
        CancellationToken ct = default
    );
}
