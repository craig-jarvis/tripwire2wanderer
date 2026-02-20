using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
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
    .AddPolicyHandler(GetResiliencePolicy());

// Configure HttpClient for TripwireClient with Polly retry policy
builder.Services.AddHttpClient<TripwireClient>()
    .AddPolicyHandler(GetResiliencePolicy());

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
static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
{
    var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));

    var retryPolicy = Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .Or<TimeoutRejectedException>()
        .OrResult(msg => (int)msg.StatusCode >= 500
                         || msg.StatusCode == HttpStatusCode.RequestTimeout
                         || msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (outcome, timespan, retryCount, _) =>
            {
                Console.WriteLine(
                    $"Retry {retryCount} after {timespan.TotalSeconds:F1}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
            });

    return Policy.WrapAsync(retryPolicy, timeoutPolicy);
}