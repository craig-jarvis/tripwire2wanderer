using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Tripwire2Wanderer;
using Tripwire2Wanderer.Clients;
using Tripwire2Wanderer.HealthChecks;
using Tripwire2Wanderer.Services;

// Load configuration from .env and environment variables
var config = Config.Load();

// Build host with dependency injection
var builder = Host.CreateApplicationBuilder(args);

// Register configuration as singleton
builder.Services.AddSingleton(config);

builder.Services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Warning);
    config.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
});

// Configure HttpClient for WandererClient with Polly retry policy
builder.Services.AddHttpClient<WandererClient>()
	.AddPolicyHandler(GetRetryPolicy());

// Configure HttpClient for TripwireClient with Polly retry policy
builder.Services.AddHttpClient<TripwireClient>()
	.AddPolicyHandler(GetRetryPolicy());

// Register health checks
builder.Services.AddHealthChecks()
	.AddCheck<TripwireHealthCheck>("tripwire_api")
	.AddCheck<WandererHealthCheck>("wanderer_api");

// Register the background service
builder.Services.AddHostedService<SyncService>();

var host = builder.Build();

// Run the application
await host.RunAsync();

// Define Polly retry policy for transient HTTP errors
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
	return HttpPolicyExtensions
		.HandleTransientHttpError()
		.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
		.WaitAndRetryAsync(
			retryCount: 5,
			sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
			onRetry: (outcome, timespan, retryCount, _) =>
			{
				Console.WriteLine($"Retry {retryCount} after {timespan.TotalSeconds:F1}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
			});
}