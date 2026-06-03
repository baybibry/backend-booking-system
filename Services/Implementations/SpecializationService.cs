using BookingSystem.Dtos.Specialization;
using BookingSystem.Entities;
using BookingSystem.Repositories.Interfaces;
using BookingSystem.Services.Interfaces;

namespace BookingSystem.Services.Implementations;

public class SpecializationService(ISpecializationRepository specializationRepo) : ISpecializationService
{
    public async Task<SpecializationResponse> CreateAsync(
        CreateSpecializationRequest request,
        CancellationToken ct = default
    )
    {
        if (await specializationRepo.ExistsByNameAsync(request.SpecializationName, ct: ct))
            throw new InvalidOperationException($"Specialization '{request.SpecializationName}' already exists.");

        var specialization = new Specialization
        {
            SpecializationName = request.SpecializationName
        };

        var created = await specializationRepo.CreateAsync(specialization, ct);
        return MapToResponse(created);
    }

    public async Task<SpecializationResponse> GetByIdAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        var specialization = await specializationRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Specialization not found.");

        return MapToResponse(specialization);
    }

    public async Task<List<SpecializationResponse>> GetAllAsync(
        CancellationToken ct = default
    )
    {
        var specializations = await specializationRepo.GetAllAsync(ct);
        return specializations.Select(MapToResponse).ToList();
    }

    public async Task<SpecializationResponse> UpdateAsync(
        Guid id,
        UpdateSpecializationRequest request,
        CancellationToken ct = default
    )
    {
        if (request.SpecializationName is not null &&
            await specializationRepo.ExistsByNameAsync(request.SpecializationName, excludeId: id, ct: ct))
            throw new InvalidOperationException($"Specialization '{request.SpecializationName}' already exists.");

        var updated = await specializationRepo.UpdateAsync(id, request, ct);
        return MapToResponse(updated);
    }

    public async Task<SpecializationResponse> DeleteAsync(
        Guid id,
        CancellationToken ct = default
    )
    {
        var deleted = await specializationRepo.DeleteAsync(id, ct);
        return MapToResponse(deleted);
    }

    private static SpecializationResponse MapToResponse(Specialization s) => new()
    {
        SpecializationId = s.SpecializationId,
        SpecializationName = s.SpecializationName
    };
}
