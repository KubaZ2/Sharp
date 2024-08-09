namespace Sharp.Backend.Manager;

public interface ISandboxProvider
{
    public Task ExecuteAsync(ContainerFunction function, Stream assembly, Stream output);
}
