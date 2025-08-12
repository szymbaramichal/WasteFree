FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["WasteFree.App/WasteFree.App.csproj", "WasteFree.App/"]
COPY ["WasteFree.Business/WasteFree.Business.csproj", "WasteFree.Business/"]
COPY ["WasteFree.Core/WasteFree.Core.csproj", "WasteFree.Core/"]
COPY ["WasteFree.Infrastructure/WasteFree.Infrastructure.csproj", "WasteFree.Infrastructure/"]

# Restore jako oddzielna warstwa
RUN dotnet restore "WasteFree.App/WasteFree.App.csproj"

COPY . .

RUN dotnet build "WasteFree.App/WasteFree.App.csproj" -c Release -o /app/build
RUN dotnet publish "WasteFree.App/WasteFree.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WasteFree.App.dll"]