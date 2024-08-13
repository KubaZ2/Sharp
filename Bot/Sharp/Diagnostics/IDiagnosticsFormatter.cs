using Microsoft.CodeAnalysis;

using NetCord.Rest;

namespace Sharp.Diagnostics;

public abstract record DiagnosticsFormatResult
{
    public record Success(List<EmbedFieldProperties> Fields, IEnumerable<MessageComponentProperties>? Components) : DiagnosticsFormatResult;

    public record Expired : DiagnosticsFormatResult;
}

public interface IDiagnosticsFormatter
{
    public DiagnosticsFormatResult FormatDiagnostics(ulong operationId, bool success, int page, int embedContentLength);

    public DiagnosticsFormatResult.Success FormatDiagnostics(ulong operationId, bool success, IReadOnlyList<Diagnostic> diagnostics, int embedContentLength);
}
