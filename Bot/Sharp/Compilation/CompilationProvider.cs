namespace Sharp.Compilation;

public class CompilationProvider(ICompilerProvider compilerProvider) : ICompilationProvider
{
    public async Task<CompilationResult> CompileAsync(ulong operationId, Language language, string code, CompilationOutput? output)
    {
        var compiler = compilerProvider.GetCompiler(language);
        if (compiler is null)
            return new CompilationResult.CompilerNotFound(language);

        List<CompilationDiagnostic> diagnostics = [];
        MemoryStream assembly = new();
        var success = await compiler.CompileAsync(operationId, code, diagnostics, assembly, output);

        if (!success)
            return new CompilationResult.Fail(language, diagnostics);

        assembly.Position = 0;

        return new CompilationResult.Success(assembly, diagnostics);
    }
}
