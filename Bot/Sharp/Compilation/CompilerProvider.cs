using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace Sharp.Compilation;

public class CompilerProvider(IEnumerable<ICompiler> compilers) : ICompilerProvider
{
    private readonly FrozenDictionary<Language, ICompiler> _compilers = compilers.ToFrozenDictionary(c => c.Language);

    public IReadOnlyList<Language> SupportedLanguages => ImmutableCollectionsMarshal.AsArray(_compilers.Keys) ?? [];

    public ICompiler? GetCompiler(Language language)
    {
        return _compilers.GetValueOrDefault(language);
    }
}
