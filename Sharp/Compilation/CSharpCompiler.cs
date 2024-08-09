using System.Text;

using Basic.Reference.Assemblies;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sharp.Compilation;

public class CSharpCompiler : ICompiler
{
    private static readonly CSharpCompilationOptions _executableOptions = new(
        outputKind: OutputKind.ConsoleApplication,
        optimizationLevel: OptimizationLevel.Release,
        allowUnsafe: true);

    private static readonly CSharpCompilationOptions _libraryOptions = new(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Release,
        allowUnsafe: true);

    private static CSharpCompilationOptions GetOptions(SyntaxTree syntaxTree, CompilationOutput? output)
    {
        if (output.HasValue)
            return output.GetValueOrDefault() switch
            {
                CompilationOutput.Executable => _executableOptions,
                CompilationOutput.Library => _libraryOptions,
                _ => throw new ArgumentOutOfRangeException(nameof(output))
            };

        return syntaxTree.GetRoot().ChildNodes().Any(node => node.IsKind(SyntaxKind.GlobalStatement)) ? _executableOptions : _libraryOptions;
    }

    private static readonly MetadataReference[] _references = [.. Net80.References.All];

    private static readonly SyntaxTree _globalUsingsSyntaxTree = CreateGlobalUsingsSyntaxTree("System",
                                                                                              "System.Collections.Generic",
                                                                                              "System.IO",
                                                                                              "System.Linq",
                                                                                              "System.Net.Http",
                                                                                              "System.Threading",
                                                                                              "System.Threading.Tasks");

    private static SyntaxTree CreateGlobalUsingsSyntaxTree(params string[] namespaces)
    {
        StringBuilder stringBuilder = new();

        foreach (var @namespace in namespaces)
        {
            stringBuilder.Append("global using global::");
            stringBuilder.Append(@namespace);
            stringBuilder.Append(';');
            stringBuilder.AppendLine();
        }

        return CSharpSyntaxTree.ParseText(stringBuilder.ToString());
    }

    public Language Language => Language.CSharp;

    public ValueTask<bool> CompileAsync(string code, ICollection<Diagnostic> diagnostics, Stream assembly, CompilationOutput? output)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var compilation = CSharpCompilation.Create("_", [_globalUsingsSyntaxTree, syntaxTree], _references, GetOptions(syntaxTree, output));

        var result = compilation.Emit(assembly);

        var resultDiagnostic = result.Diagnostics;
        int length = resultDiagnostic.Length;

        for (int i = 0; i < length; i++)
            diagnostics.Add(resultDiagnostic[i]);

        return new(result.Success);
    }
}
