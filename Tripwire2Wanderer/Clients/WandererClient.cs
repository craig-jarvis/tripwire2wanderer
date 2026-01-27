using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tripwire2Wanderer.Models;

namespace Tripwire2Wanderer.Clients;

public class WandererClient
{
	private readonly string _baseUrl;
	private readonly string _mapSlug;
	private readonly string _apiKey;
	private readonly HttpClient _client;

	public WandererClient(HttpClient client, Config config)
	{
		_baseUrl = config.WandererUrl;
		_mapSlug = config.WandererMapSlug;
		_apiKey = config.WandererApiKey;
		_client = client;
		_client.Timeout = TimeSpan.FromSeconds(30);
		_client.BaseAddress = new Uri(_baseUrl);
	}

	public async Task DeleteSystemsAndConnectionsAsync(WandererSystemAndConnectionsDeleteRequest request, CancellationToken cancellationToken = default)
	{
		var url = $"{_baseUrl}/api/maps/{_mapSlug}/systems";
		var reqBody = JsonSerializer.Serialize(request);
		await MakeRequestAsync("DELETE", url, reqBody, cancellationToken);
	}

	public async Task<WandererConnectionsAndSystemsEnvelope> GetSystemsAndConnectionsAsync(CancellationToken cancellationToken = default)
	{
		var url = $"{_baseUrl}/api/maps/{_mapSlug}/systems";
		var body = await MakeRequestAsync("GET", url, null, cancellationToken);
		var response = JsonSerializer.Deserialize<WandererConnectionsAndSystemsEnvelope>(body);
		return response ?? new WandererConnectionsAndSystemsEnvelope();
	}

	public async Task<WandererConnectionAndSystemCreateResponseEnvelope> SubmitConnectionsAndSystemsAsync(
			WandererConnectionsAndSystemsEnvelope request, CancellationToken cancellationToken = default)
	{
		var url = $"{_baseUrl}/api/maps/{_mapSlug}/systems";
		var reqBody = JsonSerializer.Serialize(request);
		var body = await MakeRequestAsync("POST", url, reqBody, cancellationToken);
		var response = JsonSerializer.Deserialize<WandererConnectionAndSystemCreateResponseEnvelope>(body);
		return response ?? new WandererConnectionAndSystemCreateResponseEnvelope();
	}

	private async Task<string> MakeRequestAsync(string method, string url, string? body, CancellationToken cancellationToken = default)
	{
		var request = new HttpRequestMessage(new HttpMethod(method), url);

		// Set API key in header
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		// Set Content-Type header if body is present
		if (body != null)
		{
			request.Content = new StringContent(body, Encoding.UTF8, "application/json");
		}

		var response = await _client.SendAsync(request, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
			throw new HttpRequestException($"API returned status {response.StatusCode}: {errorBody}");
		}

		return await response.Content.ReadAsStringAsync(cancellationToken);
	}
}
