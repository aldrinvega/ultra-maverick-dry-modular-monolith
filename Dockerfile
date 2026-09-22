# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Build stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore the API and its project references first, for layer caching.
# Only the csproj files are copied, so a source-only change does not re-restore.
COPY Ultramaverick.slnx ./
COPY src/Ultramaverick.Api/Ultramaverick.Api.csproj src/Ultramaverick.Api/
COPY src/Modules/Identity/Ultramaverick.Identity.Domain/Ultramaverick.Identity.Domain.csproj src/Modules/Identity/Ultramaverick.Identity.Domain/
COPY src/Modules/Identity/Ultramaverick.Identity.Application/Ultramaverick.Identity.Application.csproj src/Modules/Identity/Ultramaverick.Identity.Application/
COPY src/Modules/Identity/Ultramaverick.Identity.Persistence/Ultramaverick.Identity.Persistence.csproj src/Modules/Identity/Ultramaverick.Identity.Persistence/
COPY src/Modules/Identity/Ultramaverick.Identity.Infrastructure/Ultramaverick.Identity.Infrastructure.csproj src/Modules/Identity/Ultramaverick.Identity.Infrastructure/
RUN dotnet restore src/Ultramaverick.Api/Ultramaverick.Api.csproj

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/Ultramaverick.Api/Ultramaverick.Api.csproj \
        --configuration Release \
        --no-restore \
        --output /app/publish \
        /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Ultramaverick.Api.dll"]
