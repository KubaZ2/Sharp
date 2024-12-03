using NetCord.Rest;

using Sharp.Compilation;

namespace Sharp.Diagnostics;

public abstract record DiagnosticsFormatResult
{
    public record Success(List<EmbedFieldProperties> Fields, IEnumerable<ComponentProperties>? Components) : DiagnosticsFormatResult;

    public record Expired : DiagnosticsFormatResult;
}

public interface IDiagnosticsFormatter
{
    public DiagnosticsFormatResult FormatDiagnostics(ulong operationId, bool success, int page, int embedContentLength);

    public DiagnosticsFormatResult.Success FormatDiagnostics(ulong operationId, bool success, IReadOnlyList<CompilationDiagnostic> diagnostics, int embedContentLength);
}
