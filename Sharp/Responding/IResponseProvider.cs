using Microsoft.CodeAnalysis;

using NetCord.Rest;

using Sharp.Compilation;
using Sharp.Decompilation;

namespace Sharp.Responding;

public interface IResponseProvider
{
    public T CompilationResultResponse<T>(ulong operationId, CompilationResult result) where T : IMessageProperties, new();

    public T DecompilationResultResponse<T>(ulong operationId, DecompilationResult result) where T : IMessageProperties, new();

    public T LanguageNotFoundResponse<T>(ulong operationId, string? language) where T : IMessageProperties, new();

    public T Error<T>(string reason) where T : IMessageProperties, new();

    public T DecompilationResponse<T>(ulong operationId, Language language, string decompiledCode, List<Diagnostic> diagnostics) where T : IMessageProperties, new();

    public T RunResponse<T>(ulong operationId, Language language, string output, List<Diagnostic> diagnostics) where T : IMessageProperties, new();
}
