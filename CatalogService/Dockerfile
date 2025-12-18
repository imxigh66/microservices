# ===== Base =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libkrb5-3 \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

# ===== Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# копируем csproj
COPY ["CatalogService/CatalogService/CatalogService.csproj", "CatalogService/CatalogService/"]

RUN dotnet restore "CatalogService/CatalogService/CatalogService.csproj"

# копируем весь проект
COPY . .

WORKDIR "/src/CatalogService/CatalogService"
RUN dotnet publish "CatalogService.csproj" -c Release -o /app/publish

# ===== Final =====
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.dll"]
