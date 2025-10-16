FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["WasteFree.App/WasteFree.Api.csproj", "WasteFree.Api/"]
COPY ["WasteFree.Business/WasteFree.Application.csproj", "WasteFree.Application/"]
COPY ["WasteFree.Shared/WasteFree.Domain.csproj", "WasteFree.Domain/"]
COPY ["WasteFree.Infrastructure/WasteFree.Infrastructure.csproj", "WasteFree.Infrastructure/"]

RUN dotnet restore "WasteFree.Api/WasteFree.Api.csproj"

COPY . .

RUN dotnet build "WasteFree.Api/WasteFree.Api.csproj" -c Release -o /app/build
RUN dotnet publish "WasteFree.Api/WasteFree.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WasteFree.Api.dll"]