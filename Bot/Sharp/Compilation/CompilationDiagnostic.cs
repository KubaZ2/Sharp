using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Sharp.Compilation;

public record CompilationDiagnostic(
    DiagnosticSeverity Severity,
    string Id,
    LinePosition Location,
    string Message);
