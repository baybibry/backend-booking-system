using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Entities;

public abstract class BasePerson : BaseDateTime
{
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool Deactivated { get; set; }
    
    [Timestamp] public byte[] RowVersion { get; set; } = null!;
}
