namespace Sharp;

public enum Language : byte
{
    [LanguageAliases("c#", "cs", "csharp")]
    CSharp,

    [LanguageAliases("vb", "vb.net", "vbnet", "visualbasic", "visualbasic.net", "visualbasicnet")]
    VisualBasic,

    [LanguageAliases("il", "cil", "msil")]
    IL,

    [LanguageAliases("x64", "x86_64", "amd64")]
    X64,

    [LanguageAliases("x86", "ia32")]
    X86,

    [LanguageAliases("arm64", "aarch64")]
    Arm64,

    [LanguageAliases("arm32", "arm", "aarch32")]
    Arm32,
}
