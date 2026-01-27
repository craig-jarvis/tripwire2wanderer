using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tripwire2Wanderer.Clients;

namespace Tripwire2Wanderer.HealthChecks;

public class WandererHealthCheck : IHealthCheck
{
	private readonly WandererClient _wandererClient;

	public WandererHealthCheck(WandererClient wandererClient)
	{
		_wandererClient = wandererClient;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		try
		{
			// Try to fetch systems/connections as a health check
			await _wandererClient.GetSystemsAndConnectionsAsync(cancellationToken);
			return HealthCheckResult.Healthy("Wanderer API is accessible");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("Wanderer API is not accessible", ex);
		}
	}
}
