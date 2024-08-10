using System.Runtime.InteropServices;

namespace Sharp.Backend;

public interface IBackendUriProvider
{
    public ValueTask<Uri?> GetUriAsync(Architecture platform, string endpoint);
}
