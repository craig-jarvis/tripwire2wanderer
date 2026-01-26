# Check if .env exists
if (-not (Test-Path .env)) {
    Write-Host "Error: .env file not found!" -ForegroundColor Red
    Write-Host "Please copy .env.example to .env and configure your settings:"
    Write-Host "  Copy-Item .env.example .env"
    exit 1
}

# Start the application with docker-compose
Write-Host "Starting Tripwire2Wanderer..." -ForegroundColor Green
docker-compose up -d

Write-Host ""
Write-Host "Container started! View logs with:" -ForegroundColor Green
Write-Host "  docker-compose logs -f"
Write-Host ""
Write-Host "Stop the container with:" -ForegroundColor Green
Write-Host "  docker-compose down"
