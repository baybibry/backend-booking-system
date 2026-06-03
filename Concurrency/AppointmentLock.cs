namespace BookingSystem.Concurrency;

public sealed class AppointmentLock : KeyedLock<Guid>, IAppointmentLock { }
