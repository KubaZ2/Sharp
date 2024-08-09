namespace Sharp.Decompilation;

public abstract record DecompilationResult
{
    public record Success(string Code) : DecompilationResult;

    public record Fail(Language Language, string? Reason = null) : DecompilationResult;

    public record DecompilerNotFound(Language Language) : DecompilationResult;
}
