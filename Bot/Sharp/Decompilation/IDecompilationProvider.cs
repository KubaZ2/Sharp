namespace Sharp.Decompilation;

public interface IDecompilationProvider
{
    public Task<DecompilationResult> DecompileAsync(Stream assembly, Language outputLanguage);
}
