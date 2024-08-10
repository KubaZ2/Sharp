using Microsoft.CodeAnalysis;

using NetCord.Rest;

namespace Sharp.Diagnostics;

public record DiagnosticsFormatResult(IEnumerable<EmbedProperties>? Embeds, IEnumerable<MessageComponentProperties>? Components, bool Expired = false);

public interface IDiagnosticsFormatter
{
    public DiagnosticsFormatResult FormatDiagnostics(ulong operationId, int page);

    public DiagnosticsFormatResult FormatDiagnostics(ulong operationId, IReadOnlyList<Diagnostic> diagnostics, bool compilationSucceeded);
}
