FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Adapters/Driving/Notification.Worker/Notification.Worker.csproj", "src/Adapters/Driving/Notification.Worker/"]
RUN dotnet restore "src/Adapters/Driving/Notification.Worker/Notification.Worker.csproj"
COPY . .
WORKDIR "/src/src/Adapters/Driving/Notification.Worker"
RUN dotnet build "Notification.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Notification.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
