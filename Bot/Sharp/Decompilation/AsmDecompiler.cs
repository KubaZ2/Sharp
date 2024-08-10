using System.Runtime.InteropServices;

using Sharp.Backend;

namespace Sharp.Decompilation;

public abstract class AsmDecompiler(IBackendProvider backendProvider) : IDecompiler
{
    public abstract Language Language { get; }

    public abstract Architecture Platform { get; }

    public async ValueTask<bool> DecompileAsync(Stream assembly, TextWriter writer)
    {
        var asm = await backendProvider.AsmAsync(Platform, assembly);
        await writer.WriteAsync(asm);

        return true;
    }
}

public class X64Decompiler(IBackendProvider backendProvider) : AsmDecompiler(backendProvider)
{
    public override Language Language => Language.X64;

    public override Architecture Platform => Architecture.X64;
}

public class X86Decompiler(IBackendProvider backendProvider) : AsmDecompiler(backendProvider)
{
    public override Language Language => Language.X86;

    public override Architecture Platform => Architecture.X86;
}

public class Arm64Decompiler(IBackendProvider backendProvider) : AsmDecompiler(backendProvider)
{
    public override Language Language => Language.Arm64;

    public override Architecture Platform => Architecture.Arm64;
}

public class Arm32Decompiler(IBackendProvider backendProvider) : AsmDecompiler(backendProvider)
{
    public override Language Language => Language.Arm32;

    public override Architecture Platform => Architecture.Arm;
}
