namespace Sharp.Compilation;

public interface ICompilationProvider
{
    public Task<CompilationResult> CompileAsync(Language language, string code, CompilationOutput? output);
}
