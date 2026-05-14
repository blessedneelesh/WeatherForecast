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
COPY ["WebApplication1/WebApplication1.csproj", "WebApplication1/"]

RUN dotnet restore "WebApplication1/WebApplication1.csproj" \
    --runtime linux-x64

# ── Stage 2: Build + Publish ─────────────────────────────────
FROM restore AS build

# Copy all source after packages are restored
COPY . .
WORKDIR "/src/WebApplication1"

RUN dotnet publish "WebApplication1.csproj" \
    --configuration Release \
    --no-restore \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/publish \
    /p:UseAppHost=false \
    /p:PublishSingleFile=false

# ── Stage 3: Final runtime image ─────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

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

WORKDIR /app

# Install curl (required by HEALTHCHECK; not present in the base image),
# then create a non-root user and group for security
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd --gid 1000 appgroup \
    && useradd --uid 1000 --gid appgroup --shell /bin/bash --create-home appuser \
    && chown -R appuser:appgroup /app

# Copy published output from build stage
COPY --from=build --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser

# ASP.NET Core configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

# Probes the existing Add endpoint. Replace with a dedicated /health endpoint
# (e.g. builder.Services.AddHealthChecks(); app.MapHealthChecks("/health"); )
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f "http://localhost:8080/WeatherForecast/add?a=1&b=1" || exit 1

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
