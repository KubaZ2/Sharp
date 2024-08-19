using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sharp.Compilation;

public class CSharpCompiler : RoslynCompiler
{
    private static readonly CSharpCompilationOptions _executableOptions = new(
        outputKind: OutputKind.ConsoleApplication,
        optimizationLevel: OptimizationLevel.Release,
        allowUnsafe: true,
        nullableContextOptions: NullableContextOptions.Enable);

    private static readonly CSharpCompilationOptions _libraryOptions = new(
        outputKind: OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Release,
        allowUnsafe: true,
        nullableContextOptions: NullableContextOptions.Enable);

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

    private static readonly SyntaxTree _globalUsingsSyntaxTree = CreateGlobalUsingsSyntaxTree("System",
                                                                                              "System.Collections.Generic",
                                                                                              "System.IO",
                                                                                              "System.Linq",
                                                                                              "System.Net.Http",
                                                                                              "System.Threading",
                                                                                              "System.Threading.Tasks",
                                                                                              "Sharp");

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

    public override Language Language => Language.CSharp;

    protected override Microsoft.CodeAnalysis.Compilation CreateCompilation(string code, CompilationOutput? output)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        return CSharpCompilation.Create("_", [_globalUsingsSyntaxTree, syntaxTree], _references, GetOptions(syntaxTree, output));
    }
}
