# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy project files first so package restore can be cached independently
# from source-code changes.
COPY src/CommunityBot.Host/CommunityBot.Host.csproj src/CommunityBot.Host/
COPY src/CommunityBot.Core/CommunityBot.Core.csproj src/CommunityBot.Core/
COPY src/CommunityBot.Application/CommunityBot.Application.csproj src/CommunityBot.Application/
COPY src/CommunityBot.Infrastructure/CommunityBot.Infrastructure.csproj src/CommunityBot.Infrastructure/
COPY src/CommunityBot.Discord/CommunityBot.Discord.csproj src/CommunityBot.Discord/

RUN dotnet restore src/CommunityBot.Host/CommunityBot.Host.csproj

# Copy the remaining source tree.
COPY . .

WORKDIR /src/src/CommunityBot.Host

RUN dotnet build CommunityBot.Host.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# Publish
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish CommunityBot.Host.csproj \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV DOTNET_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CommunityBot.Host.dll"]