using NetCord.Rest;

using Sharp.Compilation;

namespace Sharp.CompilationResponse;

public abstract record CompilationFormatResult
{
    public record Success(EmbedProperties Embed, IEnumerable<ComponentProperties>? Components) : CompilationFormatResult;

    public record Expired : CompilationFormatResult;
}

public interface ICompilationFormatter
{
    public CompilationFormatResult CompilationResponse(ulong operationId, bool success, int page);

    public CompilationFormatResult.Success CompilationResponse(ulong operationId, bool success, IReadOnlyList<CompilationDiagnostic> diagnostics);
}
