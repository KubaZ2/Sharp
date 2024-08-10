using System.Collections.Frozen;
using System.Reflection;

namespace Sharp;

public class LanguageMatcher : ILanguageMatcher
{
    private static readonly FrozenDictionary<string, Language> _languages = typeof(Language).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                                                                            .SelectMany(f => f.GetCustomAttribute<LanguageAliasesAttribute>()!.Aliases
                                                                                                              .Select(a => (Language: (Language)f.GetRawConstantValue()!, Alias: a)))
                                                                                            .ToFrozenDictionary(t => t.Alias, t => t.Language, StringComparer.OrdinalIgnoreCase);

    public Language? Match(string? language)
    {
        if (language is null)
            return Language.CSharp;

        return _languages.TryGetValue(language, out var result) ? result : null;
    }
}
