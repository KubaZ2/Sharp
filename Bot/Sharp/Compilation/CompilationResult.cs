namespace Sharp.Compilation;

public abstract record CompilationResult
{
    public record Success(Stream Assembly, List<CompilationDiagnostic> Diagnostics) : CompilationResult;

    public record Fail(Language Language, List<CompilationDiagnostic> Diagnostics) : CompilationResult;

    public record CompilerNotFound(Language Language) : CompilationResult;
}
