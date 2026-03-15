FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

USER root

# Hack to Allow Tesseract to work on Ubuntu 
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libleptonica-dev \
        libtesseract-dev \
    && rm -rf /var/lib/apt/lists/*

RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

WORKDIR /app/x64

RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so

# Switch back to the non-root user
USER $APP_UID


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Inquisitor/Inquisitor.csproj", "Inquisitor/"]
COPY ["ExtremistDetector.Contracts/ExtremistDetector.Contracts.csproj", "ExtremistDetector.Contracts/"]
COPY ["ExtremistDetector.Infrastructure/ExtremistDetector.Infrastructure.csproj", "ExtremistDetector.Infrastructure/"]
RUN dotnet restore "Inquisitor/Inquisitor.csproj"
COPY . .
WORKDIR "/src/Inquisitor"
RUN dotnet build "./Inquisitor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Inquisitor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inquisitor.dll"]
