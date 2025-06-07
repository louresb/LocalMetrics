FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/LocalMetrics.Api/LocalMetrics.Api.csproj", "src/LocalMetrics.Api/"]
RUN dotnet restore "./src/LocalMetrics.Api/LocalMetrics.Api.csproj"
COPY . .
WORKDIR "/src/src/LocalMetrics.Api"
RUN dotnet build "./LocalMetrics.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LocalMetrics.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LocalMetrics.Api.dll"]