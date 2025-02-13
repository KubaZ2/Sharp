using Basic.Reference.Assemblies;

using Microsoft.CodeAnalysis;

namespace Sharp.Compilation;

public abstract class RoslynCompiler : ICompiler
{
    protected static readonly MetadataReference[] _references = [.. Net90.References.All, MetadataReference.CreateFromFile(typeof(JitGenericAttribute).Assembly.Location)];

    public abstract Language Language { get; }

    protected abstract Microsoft.CodeAnalysis.Compilation CreateCompilation(string code, CompilationOutput? output);

    public ValueTask<bool> CompileAsync(ulong operationId, string code, ICollection<CompilationDiagnostic> diagnostics, Stream assembly, CompilationOutput? output)
    {
        var compilation = CreateCompilation(code, output);

        var result = compilation.Emit(assembly);

        var resultDiagnostics = result.Diagnostics;
        int length = resultDiagnostics.Length;

        for (int i = 0; i < length; i++)
        {
            var resultDiagnostic = resultDiagnostics[i];

            diagnostics.Add(new(resultDiagnostic.Severity,
                                resultDiagnostic.Id,
                                resultDiagnostic.Location.GetMappedLineSpan().Span.Start,
                                resultDiagnostic.GetMessage()));
        }

        return new(result.Success);
    }
}
