#!/bin/bash

# Check if .env exists
if [ ! -f .env ]; then
    echo "Error: .env file not found!"
    echo "Please copy .env.example to .env and configure your settings:"
    echo "  cp .env.example .env"
    exit 1
fi

# Start the application with docker-compose
echo "Starting Tripwire2Wanderer..."
docker-compose up -d

echo ""
echo "Container started! View logs with:"
echo "  docker-compose logs -f"
echo ""
echo "Stop the container with:"
echo "  docker-compose down"
