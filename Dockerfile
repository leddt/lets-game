#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build

RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_14.x | bash \
    && apt-get install nodejs -yq

WORKDIR /src
COPY ["LetsGame.Web/LetsGame.Web.csproj", "LetsGame.Web/"]
RUN dotnet restore "LetsGame.Web/LetsGame.Web.csproj"
COPY . .
WORKDIR "/src/LetsGame.Web"
RUN dotnet build "LetsGame.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LetsGame.Web.csproj" --no-build -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LetsGame.Web.dll"]