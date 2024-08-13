using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

using NetCord;

using NetCord.Rest;

using Sharp.Diagnostics;

namespace Sharp.CompilationResponse;

public class CompilationFormatter(IDiagnosticsFormatter diagnosticsFormatter, IOptions<Options> options) : ICompilationFormatter
{
    private (string Title, string Description) GetContent(bool success)
    {
        var optionsValue = options.Value;

        return success
            ? ($"{optionsValue.Emojis.Success} Compilation succeeded", "The compilation succeeded.")
            : ($"{optionsValue.Emojis.Error} Compilation failed", "The compilation failed.");
    }

    public CompilationFormatResult CompilationResponse(ulong operationId, bool success, int page)
    {
        var (title, description) = GetContent(success);

        var formatResult = diagnosticsFormatter.FormatDiagnostics(operationId, success, page, title.Length + description.Length);

        return formatResult switch
        {
            DiagnosticsFormatResult.Success { Components: var components } successResult
                => new CompilationFormatResult.Success(CreateCompilationEmbed(operationId, successResult, title, description, page), components),
            DiagnosticsFormatResult.Expired => new CompilationFormatResult.Expired(),
            _ => throw new InvalidOperationException("The diagnostics format result is invalid."),
        };
    }

    public CompilationFormatResult.Success CompilationResponse(ulong operationId, bool success, IReadOnlyList<Diagnostic> diagnostics)
    {
        var (title, description) = GetContent(success);

        var formatResult = diagnosticsFormatter.FormatDiagnostics(operationId, success, diagnostics, title.Length + description.Length);

        var embed = CreateCompilationEmbed(operationId, formatResult, title, description, 1);

        return new CompilationFormatResult.Success(embed, formatResult.Components);
    }

    private EmbedProperties CreateCompilationEmbed(ulong operationId, DiagnosticsFormatResult.Success formatResult, string title, string description, int page)
    {
        var optionsValue = options.Value;

        var fields = formatResult.Fields;

        EmbedProperties embed = new()
        {
            Title = title,
            Description = description,
            Color = new(optionsValue.PrimaryColor),
            Fields = fields,
            Timestamp = Snowflake.CreatedAt(operationId),
        };

        if (fields.Count is not 0)
            embed.Footer = new EmbedFooterProperties().WithText($"Page {page}");

        return embed;
    }
}
