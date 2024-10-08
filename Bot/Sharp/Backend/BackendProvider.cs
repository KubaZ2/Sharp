using System.Buffers;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Extensions.Options;

namespace Sharp.Backend;

public class BackendProvider(IBackendUriProvider uriProvider, IHttpClientFactory clientFactory, IOptions<Options> options) : IBackendProvider
{
    private const int CopyBufferSize = 81920 / sizeof(char);

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

            return await ReadAsStringAsync(response.Content);
        }

        throw new InvalidOperationException("The backend is currently overloaded. Please try again later.");
    }

    private async ValueTask<string> ReadAsStringAsync(HttpContent content)
    {
        StringBuilder builder = new();

        using var stream = await content.ReadAsStreamAsync();
        using StreamReader reader = new(stream);

        int remaining = options.Value.MaxFileSize;
        var buffer = ArrayPool<char>.Shared.Rent(CopyBufferSize);
        int bufferLength = buffer.Length;
        int read;

        try
        {
            while (true)
            {
                read = await reader.ReadAsync(buffer.AsMemory(0, Math.Min(bufferLength, remaining)));

                if (read is 0)
                    break;

                builder.Append(buffer.AsMemory(0, read));

                if ((remaining -= read) is 0)
                    break;
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }

        return builder.ToString();
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
