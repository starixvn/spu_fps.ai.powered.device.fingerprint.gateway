FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src

# Copy solution file first
COPY ["FPS.AI.Powered.Device.Fingerprint.Gateway.sln", "./"]

# Copy project files
COPY ["src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway/Device.FingerPrint.Gateway.csproj", "src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway/"]
COPY ["src/Device.FingerPrint.Gateway.Tests/Device.FingerPrint.Gateway.Tests.csproj", "src/Device.FingerPrint.Gateway.Tests/"]

# Restore dependencies
RUN dotnet restore "src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway/Device.FingerPrint.Gateway.csproj"

# Copy source code
COPY ["src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway/", "src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway/"]

# Set working directory to the main project
WORKDIR "/src/src/FPS.AI.Powered.Device.Fingerprint.Gateway/Device.FingerPrint.Gateway"

# Build and publish the application
RUN dotnet publish -c Release -o /app --no-restore --runtime linux-x64 --self-contained false

# Runtime stage - use .NET 8.0 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

# Install required packages for timezone support
RUN apk add --no-cache tzdata

WORKDIR /app

# Copy the published application
COPY --from=builder /app .

# Set timezone
ENV TZ="America/La_Paz"

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/healthz || exit 1

# Expose port
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "Device.FingerPrint.Gateway.dll"]
