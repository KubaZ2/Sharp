namespace Sharp.Decompilation;

public interface IDecompilerProvider
{
    public IDecompiler? GetDecompiler(Language language);
}
