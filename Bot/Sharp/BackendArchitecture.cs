namespace Sharp;

// In sync with System.Runtime.InteropServices.Architecture

public enum BackendArchitecture
{
    X86 = 0,
    IA32 = X86,

    X64 = 1,
    X86_64 = X64,
    Amd64 = X64,

    Arm32 = 2,
    Arm = Arm32,
    Aarch32 = Arm32,

    Arm64 = 3,
    Aarch64 = Arm64,
}
