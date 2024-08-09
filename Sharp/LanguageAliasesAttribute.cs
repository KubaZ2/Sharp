
namespace Sharp;

[AttributeUsage(AttributeTargets.Field)]
public class LanguageAliasesAttribute(params string[] aliases) : Attribute
{
    public string[] Aliases { get; } = aliases;
}
