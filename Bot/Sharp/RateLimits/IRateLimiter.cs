namespace Sharp.RateLimits;

public interface IRateLimiter
{
    public ValueTask<bool> TryAcquireAsync(ulong id);
}
