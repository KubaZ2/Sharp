using Microsoft.CodeAnalysis;


namespace Sharp.Compilation;

public abstract record CompilationResult
{
    public record Success(Stream Assembly, List<Diagnostic> Diagnostics) : CompilationResult;

    public record Fail(Language Language, List<Diagnostic> Diagnostics) : CompilationResult;

    public record CompilerNotFound(Language Language) : CompilationResult;
}
