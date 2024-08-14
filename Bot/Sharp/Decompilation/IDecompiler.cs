namespace Sharp.Decompilation;

public interface IDecompiler
{
    public Language Language { get; }

    public ValueTask<bool> DecompileAsync(ulong operationId, Stream assembly, TextWriter writer);
}
