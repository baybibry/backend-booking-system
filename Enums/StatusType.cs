using System.Text.Json.Serialization;

namespace BookingSystem.Enums;

[JsonConverter( typeof(JsonStringEnumConverter))]
public enum StatusType
{
    Success,
    Error
}