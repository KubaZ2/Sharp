using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Sharp.RateLimits;

public class RateLimiter(IMemoryCache cache, IOptions<Options> options) : IRateLimiter
{
    private class RateLimitEntry(int remaining)
    {
        private readonly object _lock = new();

        public int Remaining => remaining;

        public bool TryAcquire()
        {
            lock (_lock)
            {
                if (remaining is 0)
                    return false;

                remaining--;
                return true;
            }
        }
    }

    private readonly object _lock = new();

    public ValueTask<bool> TryAcquireAsync(ulong id)
    {
        RateLimitEntry entry;
        lock (_lock)
        {
            entry = cache.GetOrCreate<RateLimitEntry>(id, entry =>
            {
                var rateLimits = options.Value.Backend.RateLimits;
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(rateLimits.DurationSeconds);
                return new(rateLimits.Limit);
            })!;
        }

        return new(entry.TryAcquire());
    }
}
