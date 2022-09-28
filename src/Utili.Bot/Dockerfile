FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY Utili.Bot/Utili.Bot.csproj Utili.Bot/
COPY Utili.Database/Utili.Database.csproj Utili.Database/
COPY NuGet.Config Utili.Bot/
RUN dotnet restore Utili.Bot/Utili.Bot.csproj
COPY . .
WORKDIR /src/Utili.Bot
RUN dotnet build Utili.Bot.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Utili.Bot.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utili.Bot.dll"]
