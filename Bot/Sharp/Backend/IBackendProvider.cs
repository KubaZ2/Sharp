using System.Runtime.InteropServices;

namespace Sharp.Backend;

public interface IBackendProvider
{
    public Task<string> AsmAsync(Architecture platform, Stream assembly);

    public Task<string> RunAsync(Architecture platform, Stream assembly);
}
