#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
 
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_22.x | bash \
    && apt-get install nodejs -yq

WORKDIR /src
COPY ["LetsGame.Web/LetsGame.Web.csproj", "LetsGame.Web/"]
RUN dotnet restore "LetsGame.Web/LetsGame.Web.csproj"

COPY ["LetsGame.Frontend/package.json", "LetsGame.Frontend/package-lock.json", "LetsGame.Frontend/"]
WORKDIR "/src/LetsGame.Frontend"
RUN npm ci

WORKDIR /src
COPY . .

WORKDIR "/src/LetsGame.Frontend"
RUN npm run build

WORKDIR "/src/LetsGame.Web"
RUN dotnet build "LetsGame.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LetsGame.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LetsGame.Web.dll"]