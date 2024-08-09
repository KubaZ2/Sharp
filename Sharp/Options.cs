using System.ComponentModel.DataAnnotations;

namespace Sharp;

public class Options
{
    [Required]
    public required EmojiOptions Emojis { get; set; }

    [Required]
    public required int PrimaryColor { get; set; }

    [Required]
    public required string[] BackendUris { get; set; }

    [Required]
    public required BackendArchitecture DefaultArchitecture { get; set; }

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

public class FormattingOptions
{
    public string Indentation { get; set; } = "    ";
}
