FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

ARG APP_UID=1111
RUN useradd -m -u $APP_UID appuser
USER appuser

WORKDIR /app
 
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY source/HalfLifeStats.Server/HalfLifeStats.Server.csproj HalfLifeStats.Server/

RUN dotnet restore HalfLifeStats.Server/HalfLifeStats.Server.csproj

COPY source/ ./

WORKDIR /src/HalfLifeStats.Server
RUN dotnet build HalfLifeStats.Server.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish HalfLifeStats.Server.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HalfLifeStats.Server.dll"]