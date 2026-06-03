using BookingSystem.Dtos.Common;

namespace BookingSystem.Dtos.Doctor;

public class DoctorSearchRequest : PaginationRequest
{
    public string? Search { get; set; }
    public Guid? SpecializationId { get; set; }
}
