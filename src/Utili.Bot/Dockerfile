FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Utili.Bot/Utili.Bot.csproj Utili.Bot/
COPY Utili.Database/Utili.Database.csproj Utili.Database/
COPY NuGet.Config Utili.Bot/
RUN dotnet restore Utili.Bot/Utili.Bot.csproj
COPY . .
WORKDIR /src/Utili.Bot
RUN dotnet build Utili.Bot.csproj -c Release -o /app/build

FROM build AS publish
ARG TARGETARCH
RUN dotnet publish Utili.Bot.csproj -c Release -o /app/publish -a $TARGETARCH

FROM base AS final
RUN apt-get update && apt-get install -y procps && apt-get clean
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Utili.Bot.dll"]
