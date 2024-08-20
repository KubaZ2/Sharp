using System.ComponentModel.DataAnnotations;

namespace Sharp;

public class Options
{
    [Required]
    public required EmojiOptions Emojis { get; set; }

    [Required]
    public required int PrimaryColor { get; set; }

    [Required]
    public required BackendOptions Backend { get; set; }

    public FormattingOptions Formatting { get; set; } = new();
}

public class EmojiOptions
{
    [Required]
    public required DiagnosticEmojiOptions Diagnostics { get; set; }

    [Required]
    public required string Success { get; set; }

    [Required]
    public required string Error { get; set; }

    [Required]
    public required string Help { get; set; }

    [Required]
    public required string Command { get; set; }

    [Required]
    public required string Support { get; set; }

    [Required]
    public required string Example { get; set; }
}

public class DiagnosticEmojiOptions
{
    [Required]
    public required string Info { get; set; }

    [Required]
    public required string Warning { get; set; }

    [Required]
    public required string Error { get; set; }
}

public class BackendOptions
{
    [Required]
    public required string[] Uris { get; set; }

    [Required]
    public required BackendArchitecture DefaultArchitecture { get; set; }

    public BackendRateLimitOptions RateLimits { get; set; } = new();
}

public class BackendRateLimitOptions
{
    public int Limit { get; set; } = 5;

    public int DurationSeconds { get; set; } = 30;
}

public class FormattingOptions
{
    public string Indentation { get; set; } = "    ";
}
