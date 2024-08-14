using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Mobius.ILasm.Core;
using Mobius.ILasm.interfaces;

namespace Sharp.Compilation;

public class ILCompiler : ICompiler
{
    public Language Language => Language.IL;

    private static Driver.Target GetTarget(CompilationOutput? output)
    {
        if (output.HasValue)
            return output.GetValueOrDefault() switch
            {
                CompilationOutput.Executable => Driver.Target.Exe,
                CompilationOutput.Library => Driver.Target.Dll,
                _ => throw new ArgumentOutOfRangeException(nameof(output))
            };

        return Driver.Target.Dll;
    }

    public ValueTask<bool> CompileAsync(ulong operationId, string code, ICollection<Diagnostic> diagnostics, Stream assembly, CompilationOutput? output)
    {
        CompilationLogger logger = new(diagnostics);
        Driver driver = new(logger, GetTarget(output), new() { ResourceResolver = NullManifestResourceResolver.Default });

        if (assembly is MemoryStream ms)
            return new(Compile(driver, code, ms));

        return CompileAndCopyAsync(driver, code, assembly);

        static async ValueTask<bool> CompileAndCopyAsync(Driver driver, string code, Stream assembly)
        {
            MemoryStream ms = new();
            var success = Compile(driver, code, ms);
            ms.Position = 0;
            await ms.CopyToAsync(assembly);
            return success;
        }

        static bool Compile(Driver driver, string code, MemoryStream assembly)
        {
            try
            {
                driver.Assemble([code], assembly);
            }
            catch (Exception ex) when (ex.GetType().Name == "yyException")
            {
                return false;
            }

            return true;
        }
    }

    private class CompilationLogger(ICollection<Diagnostic> diagnostics) : ILogger
    {
        public void Error(string message)
        {
            Add(null, message, DiagnosticSeverity.Error);
        }

        public void Error(Mono.ILASM.Location location, string message)
        {
            Add(location, message, DiagnosticSeverity.Error);
        }

        public void Info(string message)
        {
        }

        public void Warning(string message)
        {
            Add(null, message, DiagnosticSeverity.Warning);
        }

        public void Warning(Mono.ILASM.Location location, string message)
        {
            Add(location, message, DiagnosticSeverity.Warning);
        }

        public void Add(Mono.ILASM.Location? location, string message, DiagnosticSeverity severity)
        {
            Location? l;
            if (location is null)
                l = null;
            else
            {
                LinePosition position = new(location.line - 1, location.column - 1);
                l = Location.Create(string.Empty, default, new(position, position));
            }

            diagnostics.Add(Diagnostic.Create("IL", "IL", message, severity, severity, true, severity is DiagnosticSeverity.Warning ? 1 : 0, location: l));
        }
    }

    private class NullManifestResourceResolver : IManifestResourceResolver
    {
        public static NullManifestResourceResolver Default { get; } = new();

        public bool TryGetResourceBytes(string path, out byte[] bytes, out string error)
        {
            bytes = [];
            error = "Manifest is not available.";
            return false;
        }
    }
}
