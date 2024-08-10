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
    public CompilationFormatResult CompilationResponse(ulong operationId, int page, bool success);

    public CompilationFormatResult.Success CompilationResponse(ulong operationId, IReadOnlyList<Diagnostic> diagnostics, bool success);
}
