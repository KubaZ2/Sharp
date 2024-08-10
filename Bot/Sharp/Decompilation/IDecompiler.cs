namespace Sharp.Decompilation;

public interface IDecompiler
{
    public Language Language { get; }

    public ValueTask<bool> DecompileAsync(Stream assembly, TextWriter writer);
}
