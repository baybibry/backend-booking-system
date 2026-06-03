using System.Text.Json.Serialization;

namespace BookingSystem.Enums;

[JsonConverter( typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    AppointmentBooked,
    AppointmentConfirmed,
    AppointmentCancelled,
    AppointmentCompleted,
    AppointmentRescheduled,
    AppointmentAcceptedByReceptionist,
    AppointmentRescheduledByReceptionist,
    AppointmentArrived,
    General
}
