# -----------------------------------------------------------------------------
# Build stage
# -----------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first to preserve the NuGet restore cache.
COPY ["Algocura.LiteratureIntelligence.Api/Algocura.LiteratureIntelligence.Api.csproj", "Algocura.LiteratureIntelligence.Api/"]

COPY ["Algocura.Platform.Auditing/Algocura.Platform.Auditing.csproj", "Algocura.Platform.Auditing/"]

COPY ["Algocura.Platform.Messaging/Algocura.Platform.Messaging.csproj", "Algocura.Platform.Messaging/"]

COPY ["Algocura.Platform.Security/Algocura.Platform.Security.csproj", "Algocura.Platform.Security/"]

# Restore inside the Linux container.
RUN --mount=type=cache,id=literature-intelligence,target=/root/.nuget/packages \
    dotnet restore \
    "Algocura.LiteratureIntelligence.Api/Algocura.LiteratureIntelligence.Api.csproj"

# Copy source files. The repository-root .dockerignore prevents host
# bin/obj files from overwriting the Linux restore output.
COPY . .

WORKDIR /src/Algocura.LiteratureIntelligence.Api

RUN --mount=type=cache,id=literature-intelligence,target=/root/.nuget/packages \
   dotnet publish \
    "Algocura.LiteratureIntelligence.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


# -----------------------------------------------------------------------------
# Runtime stage
# -----------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10-resolute AS final

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Algocura.LiteratureIntelligence.Api.dll"]