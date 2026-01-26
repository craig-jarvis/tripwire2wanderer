# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY Tripwire2Wanderer/Tripwire2Wanderer.csproj Tripwire2Wanderer/

# Restore dependencies
RUN dotnet restore Tripwire2Wanderer/Tripwire2Wanderer.csproj

# Copy source code
COPY Tripwire2Wanderer/ Tripwire2Wanderer/

# Build the application
RUN dotnet publish Tripwire2Wanderer/Tripwire2Wanderer.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Set environment variables with defaults
ENV POLL_INTERVAL_SECONDS=60

# Run the application
ENTRYPOINT ["dotnet", "Tripwire2Wanderer.dll"]
