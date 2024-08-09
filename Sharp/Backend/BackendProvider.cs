using System.Runtime.InteropServices;


namespace Sharp.Backend;

public class BackendProvider(IBackendUriProvider uriProvider, IHttpClientFactory clientFactory) : IBackendProvider
{
    public async Task<string> CommonAsync(Architecture platform, Stream assembly, string endpoint)
    {
        var uri = await uriProvider.GetUriAsync(platform, endpoint) ?? throw new InvalidOperationException("No backend available for the specified platform.");

        HttpResponseMessage response;
        using (var client = clientFactory.CreateClient())
            response = await client.PostAsync(uri, new StreamContent(assembly));

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"The backend returned {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");

        return await response.Content.ReadAsStringAsync();
    }

    public Task<string> AsmAsync(Architecture platform, Stream assembly)
    {
        return CommonAsync(platform, assembly, "asm");
    }

    public Task<string> RunAsync(Architecture platform, Stream assembly)
    {
        return CommonAsync(platform, assembly, "run");
    }
}
