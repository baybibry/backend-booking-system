using BookingSystem.Enums;
using BookingSystem.Wrapper;

namespace BookingSystem.Dtos.Common;

public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => Size > 0 ? (int)Math.Ceiling((double)TotalCount / Size) : 0;

    public ResponseWrapper<PagedResponse<T>> ToWrapper(string message = "Success") =>
        ResponseWrapper<PagedResponse<T>>.On(this, message, StatusType.Success, 200);
}
