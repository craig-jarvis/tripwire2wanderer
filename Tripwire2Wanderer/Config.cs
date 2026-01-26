namespace Tripwire2Wanderer;

public class Config
{
	public string WandererUrl { get; set; } = string.Empty;
	public string WandererApiKey { get; set; } = string.Empty;
	public string WandererMapSlug { get; set; } = string.Empty;
	public int WandererCharId { get; set; }
	public int WandererHomeSystemId { get; set; }
	public string TripwireUrl { get; set; } = string.Empty;
	public string TripwireUser { get; set; } = string.Empty;
	public string TripwirePassword { get; set; } = string.Empty;
	public string TripwireMaskId { get; set; } = string.Empty;
	public int PositionXSeparation { get; set; } = 195;
	public int PositionYSeparation { get; set; } = 60;

	public static Config Load()
	{
		// Load .env file if it exists
		DotNetEnv.Env.Load();

		var config = new Config
		{
			WandererUrl = GetEnv("WANDERER_URL", "").TrimEnd('/'),
			WandererApiKey = GetEnv("WANDERER_API_KEY", ""),
			WandererMapSlug = GetEnv("WANDERER_MAP_SLUG", ""),
			WandererCharId = ParseIntEnv("WANDERER_CHAR_ID", true),
			WandererHomeSystemId = ParseIntEnv("WANDERER_HOME_SYSTEM_ID", true),
			TripwireUrl = GetEnv("TW_URL", "").TrimEnd('/'),
			TripwireUser = GetEnv("TW_USER", ""),
			TripwirePassword = GetEnv("TW_PASSWORD", ""),
			TripwireMaskId = GetEnv("TW_MASK_ID", ""),
			PositionXSeparation = 195,
			PositionYSeparation = 60
		};

		config.Validate();
		return config;
	}

	private static string GetEnv(string key, string defaultValue)
	{
		var value = Environment.GetEnvironmentVariable(key);
		return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
	}

	private static int ParseIntEnv(string key, bool required)
	{
		var val = GetEnv(key, "");
		if (string.IsNullOrWhiteSpace(val))
		{
			if (required)
			{
				throw new InvalidOperationException($"{key} is required");
			}
			return 0;
		}

		if (!int.TryParse(val, out int intVal))
		{
			throw new InvalidOperationException($"{key} must be a valid integer");
		}

		if (intVal <= 0)
		{
			throw new InvalidOperationException($"{key} must be positive, got {intVal}");
		}

		return intVal;
	}

	private void Validate()
	{
		if (string.IsNullOrWhiteSpace(WandererUrl))
			throw new InvalidOperationException("WANDERER_URL is required");

		if (!Uri.TryCreate(WandererUrl, UriKind.Absolute, out _))
			throw new InvalidOperationException("Invalid WANDERER_URL");

		if (string.IsNullOrWhiteSpace(WandererApiKey))
			throw new InvalidOperationException("WANDERER_API_KEY is required");

		if (string.IsNullOrWhiteSpace(WandererMapSlug))
			throw new InvalidOperationException("WANDERER_MAP_SLUG is required");

		if (string.IsNullOrWhiteSpace(TripwireUrl))
			throw new InvalidOperationException("TW_URL is required");

		if (!Uri.TryCreate(TripwireUrl, UriKind.Absolute, out _))
			throw new InvalidOperationException("Invalid TW_URL");

		if (string.IsNullOrWhiteSpace(TripwireUser))
			throw new InvalidOperationException("TW_USER is required");

		if (string.IsNullOrWhiteSpace(TripwirePassword))
			throw new InvalidOperationException("TW_PASSWORD is required");

		if (string.IsNullOrWhiteSpace(TripwireMaskId))
			throw new InvalidOperationException("TW_MASK_ID is required");
	}
}
