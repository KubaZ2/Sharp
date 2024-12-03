using NetCord.Rest;

using Sharp.Attachments;
using Sharp.Compilation;
using Sharp.Decompilation;

namespace Sharp.Responding;

public interface IResponseProvider
{
    public T CompilationResultResponse<T>(ulong operationId, CompilationResult result) where T : IMessageProperties, new();

    public T DecompilationResultResponse<T>(ulong operationId, DecompilationResult result) where T : IMessageProperties, new();

    public T LanguageNotFoundResponse<T>(ulong operationId, string? language) where T : IMessageProperties, new();

    public T UnknownError<T>(string reason) where T : IMessageProperties, new();

    public T DecompilationResponse<T>(ulong operationId, Language language, string decompiledCode, List<CompilationDiagnostic> diagnostics) where T : IMessageProperties, new();

    public T RunResponse<T>(ulong operationId, Language language, string output, List<CompilationDiagnostic> diagnostics) where T : IMessageProperties, new();

    public T RateLimitResponse<T>(ulong operationId) where T : IMessageProperties, new();

    public T AttachmentCodeResultResponse<T>(ulong operationId, AttachmentCodeResult result) where T : IMessageProperties, new();

    public ValueTask<T> HelpResponseAsync<T>(ulong operationId) where T : IMessageProperties, new();
}
