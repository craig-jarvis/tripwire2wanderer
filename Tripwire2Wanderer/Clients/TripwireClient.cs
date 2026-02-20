using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tripwire2Wanderer.Models;

namespace Tripwire2Wanderer.Clients;

public class TripwireClient
{
    private readonly string _baseUrl;
    private readonly HttpClient _client;
    private readonly string _maskId;
    private readonly string _password;
    private readonly string _user;

    public TripwireClient(HttpClient client, Config config)
    {
        _baseUrl = config.TripwireUrl;
        _user = config.TripwireUser;
        _password = config.TripwirePassword;
        _maskId = config.TripwireMaskId;
        _client = client;
        _client.Timeout = Timeout.InfiniteTimeSpan;
        _client.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<List<TripwireWormhole>> GetWormholesAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}?q=/wormholes&maskID={_maskId}";
        var body = await MakeRequestAsync(url, cancellationToken);
        var wormholes = JsonSerializer.Deserialize<List<TripwireWormhole>>(body);
        return wormholes ?? new List<TripwireWormhole>();
    }

    public async Task<List<TripwireSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}?q=/signatures&maskID={_maskId}";
        var body = await MakeRequestAsync(url, cancellationToken);
        var signatures = JsonSerializer.Deserialize<List<TripwireSignature>>(body);
        return signatures ?? new List<TripwireSignature>();
    }

    private async Task<string> MakeRequestAsync(string url, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Set basic authentication
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        var response = await _client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"API returned status {response.StatusCode}: {errorBody}");
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}