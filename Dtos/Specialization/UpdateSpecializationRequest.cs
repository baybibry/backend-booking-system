using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Specialization;

public class UpdateSpecializationRequest
{
    [MaxLength(150)]
    public string? SpecializationName { get; set; }
}
