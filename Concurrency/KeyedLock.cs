using System.Collections.Concurrent;

namespace BookingSystem.Concurrency;

public class KeyedLock<TKey> : IKeyedLock<TKey> where TKey : notnull, IComparable<TKey>
{
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _locks = new();

    private SemaphoreSlim GetOrCreate(TKey key) =>
        _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

    private void TryRelease(TKey key, SemaphoreSlim semaphore)
    {
        semaphore.Release();
        if (semaphore.CurrentCount == 1)
            _locks.TryRemove(key, out _);
    }

    public async Task<IDisposable> AcquireAsync(TKey key, CancellationToken ct = default)
    {
        var semaphore = GetOrCreate(key);
        await semaphore.WaitAsync(ct);
        return new Releaser(() => TryRelease(key, semaphore));
    }

    public async Task<IDisposable> AcquirePairAsync(TKey a, TKey b, CancellationToken ct = default)
    {
        var first = a.CompareTo(b) <= 0 ? a : b;
        var second = a.CompareTo(b) <= 0 ? b : a;

        var s1 = GetOrCreate(first);
        var s2 = GetOrCreate(second);

        await s1.WaitAsync(ct);
        await s2.WaitAsync(ct);

        return new Releaser(() =>
        {
            TryRelease(second, s2);
            TryRelease(first, s1);
        });
    }

    private sealed class Releaser(Action release) : IDisposable
    {
        public void Dispose() => release();
    }
}
