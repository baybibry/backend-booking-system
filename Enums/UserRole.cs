using System.Text.Json.Serialization;

namespace BookingSystem.Enums;

[JsonConverter( typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Patient,
    Doctor,
    Admin,
    Receptionist
}
