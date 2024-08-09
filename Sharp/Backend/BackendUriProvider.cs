using System.Collections.Frozen;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;

namespace Sharp.Backend;

public class BackendUriProvider : IBackendUriProvider
{
    private readonly IOptions<Options> _options;

    private readonly IHttpClientFactory _clientFactory;

    private FrozenDictionary<Architecture, IEnumerator<string>>? _uris;

    private readonly TaskCompletionSource _startCompletionSource = new();

    private readonly ILogger _logger;

    public BackendUriProvider(IOptions<Options> options, IHttpClientFactory clientFactory, ILogger<BackendUriProvider> logger)
    {
        _options = options;
        _clientFactory = clientFactory;
        _logger = logger;

        _ = SetBackendUrisAsync();
    }

    private async Task SetBackendUrisAsync()
    {
        _uris = await GetBackendUrisAsync();
        _startCompletionSource.SetResult();
    }

    private async Task<FrozenDictionary<Architecture, IEnumerator<string>>> GetBackendUrisAsync()
    {
        Dictionary<Architecture, List<string>> result = [];

        var backendUris = _options.Value.BackendUris;

        using var client = _clientFactory.CreateClient();

        int backendUrisLength = backendUris.Length;
        for (int i = 0; i < backendUrisLength; i++)
        {
            var uri = backendUris[i];

            Architecture[] architectures;
            try
            {
                architectures = (await client.GetFromJsonAsync<Architecture[]>(FormatUri(uri, "platforms")))!;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get platforms from {}, ignoring the endpoint", uri);
                continue;
            }

            AddBackend(uri, result, architectures);
        }

        return result.ToFrozenDictionary(p => p.Key, p => CreateEndlessEnumerator(p.Value));

        static IEnumerator<string> CreateEndlessEnumerator(IReadOnlyList<string> uris)
        {
            int urisCount = uris.Count;
            int i = 0;
            while (true)
            {
                yield return uris[i];
                i = (i + 1) % urisCount;
            }
        }
    }

    private static void AddBackend(string baseUri, Dictionary<Architecture, List<string>> result, Architecture[] architectures)
    {
        var architecturesLength = architectures.Length;
        for (int j = 0; j < architecturesLength; j++)
        {
            var architecture = architectures[j];

            var uri = FormatUriString(baseUri, ((int)architecture).ToString());

            if (result.TryGetValue(architecture, out var list))
                list.Add(uri);
            else
                result[architecture] = [uri];
        }
    }

    public async ValueTask<Uri?> GetUriAsync(Architecture platform, string endpoint)
    {
        await _startCompletionSource.Task;

        if (!_uris!.TryGetValue(platform, out var uris))
            return null;

        lock (uris)
        {
            uris.MoveNext();
            return FormatUri(uris.Current, endpoint);
        }
    }

    private static Uri FormatUri(ReadOnlySpan<char> baseUri, ReadOnlySpan<char> endpoint)
    {
        return new(FormatUriString(baseUri, endpoint));
    }

    private static string FormatUriString(ReadOnlySpan<char> baseUri, ReadOnlySpan<char> endpoint)
    {
        baseUri = baseUri.TrimEnd('/');
        endpoint.TrimStart('/');

        return $"{baseUri}/{endpoint}";
    }
}
