﻿FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

ARG APP_UID=1111
RUN useradd -m -u $APP_UID appuser
USER appuser

WORKDIR /app
 
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY source/HalfLifeMetrics.Server/HalfLifeMetrics.Server.csproj HalfLifeMetrics.Server/

RUN dotnet restore HalfLifeMetrics.Server/HalfLifeMetrics.Server.csproj

COPY source/ ./

WORKDIR /src/HalfLifeMetrics.Server
RUN dotnet build HalfLifeMetrics.Server.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish HalfLifeMetrics.Server.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HalfLifeMetrics.Server.dll"]