# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore "Erox.ImageService.Api.csproj"

COPY *.cs ./
COPY *.json ./
COPY Properties/ ./Properties/
COPY Controllers/ ./Controllers/
COPY Services/ ./Services/

RUN dotnet publish "Erox.ImageService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Устанавливаем переменную окружения для порта
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Erox.ImageService.Api.dll"]