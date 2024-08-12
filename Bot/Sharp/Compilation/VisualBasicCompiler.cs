using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Sharp.Compilation;

public class VisualBasicCompiler : RoslynCompiler
{
    static VisualBasicCompiler()
    {
        var globalImports = CreateGlobalImports("System",
                                                "Microsoft.VisualBasic",
                                                "System.Collections.Generic",
                                                "System.Linq",
                                                "System.Collections",
                                                "System.Diagnostics",
                                                "System.Threading.Tasks",
                                                "Sharp");

        _executableOptions = new(
            outputKind: OutputKind.ConsoleApplication,
            optimizationLevel: OptimizationLevel.Release,
            globalImports: globalImports);

        _libraryOptions = new(
            outputKind: OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            globalImports: globalImports);
    }

    private static GlobalImport[] CreateGlobalImports(params string[] namespaces)
    {
        int length = namespaces.Length;
        var result = new GlobalImport[length];

        for (int i = 0; i < length; i++)
            result[i] = GlobalImport.Parse(namespaces[i]);

        return result;
    }

    private static readonly VisualBasicCompilationOptions _executableOptions;

    private static readonly VisualBasicCompilationOptions _libraryOptions;

    private static VisualBasicCompilationOptions GetOptions(CompilationOutput? output)
    {
        if (output.HasValue)
            return output.GetValueOrDefault() switch
            {
                CompilationOutput.Executable => _executableOptions,
                CompilationOutput.Library => _libraryOptions,
                _ => throw new ArgumentOutOfRangeException(nameof(output))
            };

        return _libraryOptions;
    }

    public override Language Language => Language.VisualBasic;

    protected override Microsoft.CodeAnalysis.Compilation CreateCompilation(string code, CompilationOutput? output)
    {
        return VisualBasicCompilation.Create("_", [VisualBasicSyntaxTree.ParseText(code)], _references, GetOptions(output));
    }
}
