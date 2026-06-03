namespace BookingSystem.Entities;

public class Specialization : BaseDateTime
{
    public Guid SpecializationId { get; set; }
    public string SpecializationName { get; set; } = null!;

    public ICollection<Doctor> Doctors { get; set; } = [];
}
