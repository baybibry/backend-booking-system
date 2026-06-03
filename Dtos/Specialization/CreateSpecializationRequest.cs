using System.ComponentModel.DataAnnotations;
using BookingSystem.Helper;

namespace BookingSystem.Dtos.Specialization;

public class CreateSpecializationRequest
{
    [RequiredNonEmpty]
    [MaxLength(150)]
    public string SpecializationName { get; set; } = null!;
}
