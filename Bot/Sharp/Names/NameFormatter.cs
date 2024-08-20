namespace Sharp.Names;

public class NameFormatter : INameFormatter
{
    public string Format(Language language)
    {
        return language switch
        {
            Language.CSharp => "C#",
            Language.VisualBasic => "VB",
            Language.FSharp => "F#",
            Language.IL => "IL",
            Language.X64 => "x64",
            Language.X86 => "x86",
            Language.Arm64 => "ARM64",
            Language.Arm32 => "ARM32",
            _ => throw new ArgumentOutOfRangeException(nameof(language))
        };
    }

    public string Format(BackendArchitecture architecture)
    {
        return architecture switch
        {
            BackendArchitecture.X86 => "x86",
            BackendArchitecture.X64 => "x64",
            BackendArchitecture.Arm32 => "ARM32",
            BackendArchitecture.Arm64 => "ARM64",
            _ => throw new ArgumentOutOfRangeException(nameof(architecture))
        };
    }
}
