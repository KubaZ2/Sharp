using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Sharp.Backend.Manager;

public class Options
{
    [Required]
    public required HashSet<Architecture> Platforms { get; set; }
}
