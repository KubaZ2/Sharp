namespace Sharp.Compilation;

public interface ICompilerProvider
{
    public ICompiler? GetCompiler(Language language);
}
