FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Butcher/Butcher.csproj", "Butcher/"]
COPY ["ExtremistDetector.Contracts/ExtremistDetector.Contracts.csproj", "ExtremistDetector.Contracts/"]
COPY ["ExtremistDetector.Infrastructure/ExtremistDetector.Infrastructure.csproj", "ExtremistDetector.Infrastructure/"]
RUN dotnet restore "Butcher/Butcher.csproj"
COPY . .
WORKDIR "/src/Butcher"
RUN dotnet build "./Butcher.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Butcher.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Butcher.dll"]
