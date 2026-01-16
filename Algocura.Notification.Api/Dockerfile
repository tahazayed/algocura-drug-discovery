# -----------------------------------------------------------------------------
# Build stage
# -----------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files first to preserve the NuGet restore cache.
COPY ["Algocura.Notification.Api/Algocura.Notification.Api.csproj", "Algocura.Notification.Api/"]

COPY ["Algocura.Platform.Auditing/Algocura.Platform.Auditing.csproj", "Algocura.Platform.Auditing/"]

# Restore inside the Linux container.
RUN dotnet restore \
    "Algocura.Notification.Api/Algocura.Notification.Api.csproj"

# Copy source files. The repository-root .dockerignore prevents host
# bin/obj files from overwriting the Linux restore output.
COPY . .

WORKDIR /src/Algocura.Notification.Api

RUN dotnet publish \
    "Algocura.Notification.Api.csproj" \
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

ENTRYPOINT ["dotnet", "Algocura.Notification.Api.dll"]