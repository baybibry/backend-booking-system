using BookingSystem.Dtos.Specialization;
using BookingSystem.Entities;

namespace BookingSystem.Repositories.Interfaces;

public interface ISpecializationRepository
{
    Task<Specialization> CreateAsync(Specialization specialization, CancellationToken ct = default);
    Task<Specialization> UpdateAsync(Guid id, UpdateSpecializationRequest request, CancellationToken ct = default);
    Task<Specialization> DeleteAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

    Task<Specialization?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Specialization>> GetAllAsync(CancellationToken ct = default);
}
