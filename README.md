# tripwire2wanderer

Tool for performing a one-way sync from a Tripwire mask to a Wanderer map. Runs continuously in a loop, syncing at configurable intervals.

## Features

- Continuous syncing from Tripwire to Wanderer
- Configurable poll interval
- Docker support for easy deployment
- Debug mode to test without making API calls to Wanderer
- Graceful shutdown handling

## Configuration

The application is configured via environment variables. You can use a `.env` file or set them directly.

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `WANDERER_URL` | Base URL of your Wanderer instance | `https://wanderer.example.com` |
| `WANDERER_API_KEY` | API key for Wanderer authentication | `your-api-key` |
| `WANDERER_MAP_SLUG` | Slug identifier for the Wanderer map | `my-map` |
| `WANDERER_CHAR_ID` | Character ID for Wanderer | `12345678` |
| `WANDERER_HOME_SYSTEM_ID` | EVE system ID of the home system | `30000142` |
| `TW_URL` | Base URL of your Tripwire instance | `https://tripwire.example.com` |
| `TW_USER` | Tripwire username | `your-username` |
| `TW_PASSWORD` | Tripwire password | `your-password` |
| `TW_MASK_ID` | Tripwire mask ID to sync | `mask-id` |

### Optional Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `POLL_INTERVAL_SECONDS` | Seconds between sync operations | `60` |

## Running Locally

1. Copy `.env.example` to `.env` and fill in your configuration:
   ```bash
   cp .env.example .env
   ```

2. Build and run:
   ```bash
   cd Tripwire2Wanderer
   dotnet run
   ```

3. Run in debug mode (skips Wanderer API calls):
   ```bash
   dotnet run -- --debug
   ```

## Running with Docker

### Using Docker Compose (Recommended)

1. Copy `.env.example` to `.env` and configure your settings
2. Start the container:
   ```bash
   docker-compose up -d
   ```

3. View logs:
   ```bash
   docker-compose logs -f
   ```

4. Stop the container:
   ```bash
   docker-compose down
   ```

### Using Docker directly

1. Build the image:
   ```bash
   docker build -t tripwire2wanderer .
   ```

2. Run the container:
   ```bash
   docker run -d --name tripwire2wanderer \
     -e WANDERER_URL=https://wanderer.example.com \
     -e WANDERER_API_KEY=your-api-key \
     -e WANDERER_MAP_SLUG=my-map \
     -e WANDERER_CHAR_ID=12345678 \
     -e WANDERER_HOME_SYSTEM_ID=30000142 \
     -e TW_URL=https://tripwire.example.com \
     -e TW_USER=your-username \
     -e TW_PASSWORD=your-password \
     -e TW_MASK_ID=mask-id \
     -e POLL_INTERVAL_SECONDS=60 \
     tripwire2wanderer
   ```

3. Run in debug mode:
   ```bash
   docker run -d --name tripwire2wanderer \
     --env-file .env \
     tripwire2wanderer --debug
   ```

## How It Works

1. The application loads configuration from environment variables or `.env` file
2. It enters a continuous loop that:
   - Fetches wormhole connections and signatures from Tripwire
   - Builds a map structure from the data
   - (Unless in debug mode) Syncs the map to Wanderer:
     - Fetches current Wanderer data
     - **Compares the new data with existing Wanderer data**
     - **If no changes are detected, skips the update entirely** (optimization)
     - If changes are detected:
       - Deletes removed systems/connections
       - Adds/updates new systems/connections
   - Waits for the configured poll interval before the next sync
3. The loop continues until the application receives a shutdown signal (Ctrl+C or Docker stop)

## Optimizations

The application includes smart change detection to avoid unnecessary API calls:
- After building the map from Tripwire data, it compares with the current Wanderer state
- If the systems and connections are identical (no additions, deletions, or modifications), the update is skipped
- This significantly reduces API load when the map hasn't changed between polling intervals

## Debug Mode

Debug mode (`--debug` flag) allows you to test the Tripwire data fetching and map building without actually making changes to Wanderer. This is useful for:
- Testing configuration
- Validating Tripwire connectivity
- Inspecting the generated map JSON

In debug mode, the application will still fetch data from Tripwire and output the generated map JSON, but will skip all Wanderer API calls.
