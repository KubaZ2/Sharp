using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Sharp.Backend.Manager;

public class Options
{
    [Required]
    public required HashSet<Architecture> Platforms { get; set; }

    [Required]
    public required RateLimitOptions RateLimits { get; set; }

    public int MaxOutputSize { get; set; } = 1024 * 1024;
}

public class RateLimitOptions
{
    [Required]
    public required int ConcurrencyLimit { get; set; }
}
