﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY Utili.Backend/Utili.Backend.csproj Utili.Backend/
COPY Utili.Database/Utili.Database.csproj Utili.Database/
COPY NuGet.Config Utili.Backend/NuGet.Config
RUN dotnet restore Utili.Backend/Utili.Backend.csproj
COPY . .
WORKDIR /src/Utili.Backend
RUN dotnet build Utili.Backend.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Utili.Backend.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utili.Backend.dll", "--no-database-migration"]
