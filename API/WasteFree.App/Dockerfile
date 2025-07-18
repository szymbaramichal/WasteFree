FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . ./

RUN ls -la
RUN dotnet restore WasteFree.App.csproj
RUN dotnet publish WasteFree.App.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "WasteFree.App.dll"]
