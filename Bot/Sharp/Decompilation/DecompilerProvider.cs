using System.Collections.Frozen;
using System.Runtime.InteropServices;

namespace Sharp.Decompilation;

public class DecompilerProvider(IEnumerable<IDecompiler> decompilers) : IDecompilerProvider
{
    private readonly FrozenDictionary<Language, IDecompiler> _decompilers = decompilers.ToFrozenDictionary(c => c.Language);

    public IReadOnlyList<Language> SupportedLanguages => ImmutableCollectionsMarshal.AsArray(_decompilers.Keys) ?? [];

    public IDecompiler? GetDecompiler(Language language)
    {
        return _decompilers.GetValueOrDefault(language);
    }
}
