using Microsoft.CodeAnalysis;

namespace Sharp.Compilation;

public interface ICompiler
{
    public Language Language { get; }

    public ValueTask<bool> CompileAsync(string code, ICollection<Diagnostic> diagnostics, Stream assembly, CompilationOutput? output);
}
