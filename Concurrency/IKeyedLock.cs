namespace BookingSystem.Concurrency;

public interface IKeyedLock<TKey> where TKey : notnull, IComparable<TKey>
{
    Task<IDisposable> AcquireAsync(TKey key, CancellationToken ct = default);
    Task<IDisposable> AcquirePairAsync(TKey a, TKey b, CancellationToken ct = default);
}
