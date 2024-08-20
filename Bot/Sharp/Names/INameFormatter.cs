namespace Sharp.Names;

public interface INameFormatter
{
    public string Format(Language language);

    public string Format(BackendArchitecture architecture);
}
