namespace Sharp;

public class LanguageFormatProvider : ILanguageFormatProvider
{
    public string GetFormat(Language language)
    {
        return language switch
        {
            Language.CSharp or Language.IL => "cs", // Discord does not support IL extension
            Language.X64 or Language.X86 => "x86asm",
            Language.Arm64 or Language.Arm32 => "arm",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
        };
    }
}
