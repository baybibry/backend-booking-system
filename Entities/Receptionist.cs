namespace BookingSystem.Entities;

public class Receptionist : BasePerson
{
    public Guid ReceptionistId { get; set; }
    public string EmployeeNo { get; set; } = null!;
}
