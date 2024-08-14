using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;

using Microsoft.Extensions.Options;

namespace Sharp.Decompilation;

public class ILDecompiler(IOptions<Options> options) : IDecompiler
{
    private readonly string _indentationString = options.Value.Formatting.Indentation;

    public Language Language => Language.IL;

    public ValueTask<bool> DecompileAsync(ulong operationId, Stream assembly, TextWriter writer)
    {
        using PEFile peFile = new(string.Empty, new PEReader(assembly), MetadataReaderOptions.Default);

        PlainTextOutput plainTextOutput = new(writer)
        {
            IndentationString = _indentationString,
        };

        ReflectionDisassembler disassembler = new(plainTextOutput, default);

        disassembler.WriteAssemblyHeader(peFile);
        plainTextOutput.WriteLine();

        disassembler.WriteModuleContents(peFile);

        return new(true);
    }
}
