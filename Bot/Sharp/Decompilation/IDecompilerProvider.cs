namespace Sharp.Decompilation;

public interface IDecompilerProvider
{
    public IReadOnlyList<Language> SupportedLanguages { get; }
    
    public IDecompiler? GetDecompiler(Language language);
}
