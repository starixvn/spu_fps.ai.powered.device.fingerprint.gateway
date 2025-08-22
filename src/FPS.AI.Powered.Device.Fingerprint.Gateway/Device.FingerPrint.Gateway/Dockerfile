FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder

WORKDIR /src

COPY ["./src/Fanex.LogTrail.Service", "Fanex.LogTrail.Service/"]

COPY ["./src/Device.FingerPrint.Model", "Device.FingerPrint.Model/"]

COPY ["./src/Core/Device.FingerPrint.Core", "Core/Device.FingerPrint.Core/"]

COPY ["./src/Device.FingerPrint.Redis", "Device.FingerPrint.Redis/"]

COPY ["./src/Device.FingerPrint.Gateway/Device.FingerPrint.Gateway.csproj", "Device.FingerPrint.Gateway/"]

WORKDIR "/src/Device.FingerPrint.Gateway"

RUN dotnet nuget add source https://nuget.nexdev.net/api/v2

RUN dotnet restore -v n

COPY ./src/Device.FingerPrint.Gateway .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

WORKDIR /app

COPY --from=builder /app .

ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "Device.FingerPrint.Gateway.dll"]
