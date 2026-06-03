using System.ComponentModel.DataAnnotations;

namespace BookingSystem.Dtos.Common;

public class PaginationRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Size must be between 1 and 100.")]
    public int Size { get; set; } = 10;
}
