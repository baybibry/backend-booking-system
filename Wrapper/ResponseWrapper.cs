using BookingSystem.Enums;

namespace BookingSystem.Wrapper;

public class ResponseWrapper<T>
{
    public T? Data { get; set; }
    public string? Message { get; set; }
    public StatusType? Type { get; set; }
    public int StatusCode { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    
    public static ResponseWrapper<T> On(T? data, string message, StatusType statusType, int statusCode)
    {
        var currentData = data ?? default; 
        
        return new ResponseWrapper<T>
        {
            Data = currentData,
            Message = message,
            Type = statusType,
            StatusCode = statusCode,
            CreateAt = DateTime.UtcNow
        };
    }
}