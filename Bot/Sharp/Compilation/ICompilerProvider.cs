namespace Sharp.Compilation;

public interface ICompilerProvider
{
    public IReadOnlyList<Language> SupportedLanguages { get; }

    public ICompiler? GetCompiler(Language language);
}
