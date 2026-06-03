namespace BookingSystem.Dtos.Common;

public class PatientSearchRequest : PaginationRequest
{
    public string? Search { get; set; }
}
