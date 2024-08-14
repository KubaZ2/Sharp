namespace Sharp.Decompilation;

public class DecompilationProvider(IDecompilerProvider decompilerProvider) : IDecompilationProvider
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

        return new DecompilationResult.Success(writer.ToString());
    }
}
