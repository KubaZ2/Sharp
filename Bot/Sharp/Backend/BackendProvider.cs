using System.Net;
using System.Runtime.InteropServices;


namespace Sharp.Backend;

public class BackendProvider(IBackendUriProvider uriProvider, IHttpClientFactory clientFactory) : IBackendProvider
{
    public async Task<string> CommonAsync(Architecture platform, Stream assembly, string endpoint)
    {
        for (int i = 0; i < 3; i++)
        {
            var uri = await uriProvider.GetUriAsync(platform, endpoint) ?? throw new InvalidOperationException($"No backend available for the {platform} platform.");

            HttpResponseMessage response;
            using (var client = clientFactory.CreateClient())
                response = await client.PostAsync(uri, new StreamContent(assembly));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode is HttpStatusCode.TooManyRequests)
                    continue;

                var content = response.Content;
                
                var message = content.Headers.ContentLength.GetValueOrDefault() is 0
                    ? $"The backend returned {(int)response.StatusCode}."
                    : $"The backend returned {(int)response.StatusCode} {await content.ReadAsStringAsync()}.";

                throw new InvalidOperationException(message);
            }

            return await response.Content.ReadAsStringAsync();
        }

        throw new InvalidOperationException("The backend is currently overloaded. Please try again later.");
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
