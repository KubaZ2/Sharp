using Microsoft.CodeAnalysis;

using NetCord.Rest;

namespace Sharp.CompilationResponse;

public abstract record CompilationFormatResult
{
    public record Success(EmbedProperties Embed, IEnumerable<MessageComponentProperties>? Components) : CompilationFormatResult;

    public record Expired : CompilationFormatResult;
}

public interface ICompilationFormatter
{
    public CompilationFormatResult CompilationResponse(ulong operationId, bool success, int page);

    public CompilationFormatResult.Success CompilationResponse(ulong operationId, bool success, IReadOnlyList<Diagnostic> diagnostics);
}
