# Build stage
FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS build
WORKDIR /src

# Copy NuGet configuration
COPY ["NuGet.config", "."]

# Copy solution file
COPY ["FPS.AI.Powered.Device.Fingerprint.Gateway.sln", "."]

# Copy project files for dependency resolution
COPY ["src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway/Device.FingerPrint.Gateway.csproj", "src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway/"]
COPY ["src/Device.FingerPrint.Gateway.Tests/Device.FingerPrint.Gateway.Tests.csproj", "src/Device.FingerPrint.Gateway.Tests/"]

# Restore dependencies
RUN dotnet restore "FPS.AI.Powered.Device.Fingerprint.Gateway.sln"

# Copy all source code
COPY . .

# Build and publish the main application
WORKDIR "/src/src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway"
RUN dotnet publish "Device.FingerPrint.Gateway.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS runtime

WORKDIR /app

# Create non-root user for security
RUN addgroup -g 1001 -S appgroup && \
    adduser -S appuser -u 1001 -G appgroup

# Set timezone
ENV TZ="America/La_Paz"
RUN apk add --no-cache tzdata

# Copy published application
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

# Expose port 8080 (standard for containerized ASP.NET Core apps)
EXPOSE 8080

# Configure Kestrel to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/healthz || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Device.FingerPrint.Gateway.dll"]
