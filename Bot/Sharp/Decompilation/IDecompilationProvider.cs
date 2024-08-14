namespace Sharp.Decompilation;

public interface IDecompilationProvider
{
    public Task<DecompilationResult> DecompileAsync(ulong operationId, Stream assembly, Language outputLanguage);
}
