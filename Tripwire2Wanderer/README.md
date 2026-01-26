# Tripwire2Wanderer - C# Console Application

A C# console application that performs a one-way sync from a Tripwire mask to a Wanderer map.

## Requirements

- .NET 8.0 SDK or later

## Configuration

Create a `.env` file in the same directory as the executable with the following environment variables:

```env
WANDERER_URL=https://your-wanderer-instance.com
WANDERER_API_KEY=your-api-key
WANDERER_MAP_SLUG=your-map-slug
WANDERER_CHAR_ID=12345678
WANDERER_HOME_SYSTEM_ID=30000142

TW_URL=https://your-tripwire-instance.com
TW_USER=your-username
TW_PASSWORD=your-password
TW_MASK_ID=your-mask-id
```

Alternatively, you can set these as environment variables directly in your system.

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run
```

Or build and run the release version:

```bash
dotnet build -c Release
dotnet run -c Release
```

## How It Works

1. Loads configuration from `.env` file or environment variables
2. Fetches wormhole connections and signatures from Tripwire
3. Fetches current map data from Wanderer
4. Builds a new map structure recursively from the home system
5. Calculates optimal positions for systems in the map
6. Deletes systems and connections no longer present in Tripwire
7. Submits new/updated systems and connections to Wanderer

## Project Structure

- `Program.cs` - Main entry point
- `Config.cs` - Configuration loader and validator
- `DataHelpers.cs` - Helper methods for data transformation and map building
- `Models/` - Data models for Tripwire and Wanderer APIs
  - `TripwireModels.cs` - Tripwire data structures
  - `WandererModels.cs` - Wanderer data structures
- `Clients/` - API clients
  - `TripwireClient.cs` - Tripwire API client
  - `WandererClient.cs` - Wanderer API client

## Dependencies

- **DotNetEnv** (v3.1.1) - For loading `.env` files
- **System.Text.Json** (v8.0.5) - For JSON serialization/deserialization
