namespace Sharp.Compilation;

public interface ICompilationProvider
{
    public Task<CompilationResult> CompileAsync(ulong operationId, Language language, string code, CompilationOutput? output);
}
