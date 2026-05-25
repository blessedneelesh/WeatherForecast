# =============================================================
# Multi-Stage Dockerfile — WeatherForecast .NET 8 Web API
# Stage 1: restore    — cache NuGet packages as a separate layer
# Stage 2: build      — compile and publish
# Stage 3: final      — minimal ASP.NET runtime image
#
# Security hardening:
#   - Non-root user (appuser)
#   - No SDK in final image (attack surface reduction)
#   - OCI labels for traceability
# =============================================================

# ── Stage 1: Restore (cached layer) ─────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

# Copy only project files first — this layer is cached until
# any .csproj changes, giving fast rebuilds for code-only changes
COPY ["WeatherForecastApi/WeatherForecastApi.csproj", "WeatherForecastApi/"]

RUN dotnet restore "WeatherForecastApi/WeatherForecastApi.csproj" \
    --runtime linux-x64

# ── Stage 2: Build + Publish ─────────────────────────────────
FROM restore AS build

# Copy all source after packages are restored
COPY . .
WORKDIR "/src/WeatherForecastApi"

RUN dotnet build "WeatherForecastApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WeatherForecastApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ── Stage 3: Final runtime image ─────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final

# Build arguments for OCI labels (set by GitHub Actions metadata-action)
ARG BUILD_DATE
ARG VCS_REF
ARG DOTNET_VERSION=8.0

# OCI image labels for traceability
LABEL org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.revision="${VCS_REF}" \
      org.opencontainers.image.title="WeatherForecast API" \
      org.opencontainers.image.description=".NET 8 Web API" \
      org.opencontainers.image.base.name="mcr.microsoft.com/dotnet/aspnet:8.0"

# Update OS packages to get security patches
RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080

# Set environment for Azure App Service
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Non-root user for security
RUN groupadd -r appuser && useradd --no-log-init -r -g appuser appuser
USER appuser

# Copy published output from publish stage
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
