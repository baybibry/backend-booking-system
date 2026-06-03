using System.Text.Json.Serialization;

namespace BookingSystem.Enums;

[JsonConverter( typeof(JsonStringEnumConverter))]
public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled,
    Arrived,
    Expired
}
