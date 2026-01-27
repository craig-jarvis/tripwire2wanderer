using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tripwire2Wanderer.Clients;

namespace Tripwire2Wanderer.HealthChecks;

public class TripwireHealthCheck : IHealthCheck
{
	private readonly TripwireClient _tripwireClient;

	public TripwireHealthCheck(TripwireClient tripwireClient)
	{
		_tripwireClient = tripwireClient;
	}

	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		try
		{
			// Try to fetch wormholes as a health check
			await _tripwireClient.GetWormholesAsync(cancellationToken);
			return HealthCheckResult.Healthy("Tripwire API is accessible");
		}
		catch (Exception ex)
		{
			return HealthCheckResult.Unhealthy("Tripwire API is not accessible", ex);
		}
	}
}
