using System.Collections.Frozen;

namespace Sharp.Compilation;

public class CompilerProvider(IEnumerable<ICompiler> compilers) : ICompilerProvider
{
    private readonly FrozenDictionary<Language, ICompiler> _compilers = compilers.ToFrozenDictionary(c => c.Language);

    public ICompiler? GetCompiler(Language language)
    {
        return _compilers.GetValueOrDefault(language);
    }
}
