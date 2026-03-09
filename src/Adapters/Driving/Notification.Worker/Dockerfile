FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Adapters/Driving/Notification.Worker/Notification.Worker.csproj", "src/Adapters/Driving/Notification.Worker/"]
COPY ["src/Adapters/External.User.API/External.User.API.csproj", "src/Adapters/External.User.API/"]
COPY ["src/Core/Domain/Domain.csproj", "src/Core/Domain/"]
RUN dotnet restore "src/Adapters/Driving/Notification.Worker/Notification.Worker.csproj"
COPY . .
WORKDIR "/src/src/Adapters/Driving/Notification.Worker"
RUN dotnet build "./Notification.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notification.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN apk add --no-cache \
    icu-data-full \
    icu-libs \
    curl

COPY --from=publish /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
