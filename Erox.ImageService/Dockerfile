FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# FIX for SQLClient: missing libgssapi_krb5.so.2
RUN apt-get update && apt-get install -y \
    libkrb5-3 \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj
COPY Erox.ImageService/Erox.ImageService.Api/Erox.ImageService.Api.csproj Erox.ImageService.Api/

# Восстанавливаем зависимости
RUN dotnet restore "Erox.ImageService.Api/Erox.ImageService.Api.csproj"

# Копируем весь проект
COPY Erox.ImageService/Erox.ImageService.Api/ Erox.ImageService.Api/

# Собираем
WORKDIR /src/Erox.ImageService.Api
RUN dotnet publish "Erox.ImageService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Erox.ImageService.Api.dll"]
