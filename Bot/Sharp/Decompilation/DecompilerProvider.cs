using System.Collections.Frozen;

namespace Sharp.Decompilation;

public class DecompilerProvider(IEnumerable<IDecompiler> decompilers) : IDecompilerProvider
{
    private readonly FrozenDictionary<Language, IDecompiler> _decompilers = decompilers.ToFrozenDictionary(c => c.Language);

    public IDecompiler? GetDecompiler(Language language)
    {
        return _decompilers.GetValueOrDefault(language);
    }
}
