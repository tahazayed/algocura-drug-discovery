# -----------------------------------------------------------------------------
# Restore stage
# -----------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore

WORKDIR /src

COPY ["Algocura.CompoundCatalog.Api.csproj", "./"]

RUN dotnet restore "Algocura.CompoundCatalog.Api.csproj"


# -----------------------------------------------------------------------------
# Build stage
# -----------------------------------------------------------------------------

FROM restore AS build

COPY . .

RUN dotnet build "Algocura.CompoundCatalog.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/build


# -----------------------------------------------------------------------------
# Publish stage
# -----------------------------------------------------------------------------

FROM build AS publish

RUN dotnet publish "Algocura.CompoundCatalog.Api.csproj" \
    --configuration Release \
    --no-restore \
    --no-build \
    --output /app/publish \
    /p:UseAppHost=false


# -----------------------------------------------------------------------------
# Runtime stage
# -----------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:10.0.10-resolute AS final

WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=publish /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Algocura.CompoundCatalog.Api.dll"]