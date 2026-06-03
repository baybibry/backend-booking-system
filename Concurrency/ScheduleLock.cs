namespace BookingSystem.Concurrency;

public sealed class ScheduleLock : KeyedLock<Guid>, IScheduleLock { }
