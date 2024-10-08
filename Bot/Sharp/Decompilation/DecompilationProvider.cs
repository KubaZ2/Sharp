using Microsoft.Extensions.Options;

namespace Sharp.Decompilation;

public class DecompilationProvider(IDecompilerProvider decompilerProvider, IOptions<Options> options) : IDecompilationProvider
{
    public async Task<DecompilationResult> DecompileAsync(ulong operationId, Stream assembly, Language outputLanguage)
    {
        var decompiler = decompilerProvider.GetDecompiler(outputLanguage);
        if (decompiler is null)
            return new DecompilationResult.DecompilerNotFound(outputLanguage);

        StringWriter writer = new();
        var success = await decompiler.DecompileAsync(operationId, assembly, writer);

        if (!success)
            return new DecompilationResult.Fail(outputLanguage);

        var stringBuilder = writer.GetStringBuilder();

        var code = stringBuilder.ToString(0, Math.Min(stringBuilder.Length, options.Value.MaxFileSize));

        return new DecompilationResult.Success(code);
    }
}
