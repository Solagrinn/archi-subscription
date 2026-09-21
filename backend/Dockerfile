FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY ArchiSubscription.slnx .
COPY src/ArchiSubscription.API/ArchiSubscription.API.csproj src/ArchiSubscription.API/
COPY src/ArchiSubscription.Core/ArchiSubscription.Core.csproj src/ArchiSubscription.Core/
COPY src/ArchiSubscription.Infrastructure/ArchiSubscription.Infrastructure.csproj src/ArchiSubscription.Infrastructure/

RUN dotnet restore ArchiSubscription.slnx

# Copy everything else and build
COPY . .
RUN dotnet publish src/ArchiSubscription.API/ArchiSubscription.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ArchiSubscription.API.dll"]
