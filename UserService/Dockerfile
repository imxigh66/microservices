# ===== Base image =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# ===== Build image =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj
COPY ["UserService/UserService/UserService.csproj", "UserService/UserService/"]

RUN dotnet restore "UserService/UserService/UserService.csproj"

# Copy all
COPY . .

WORKDIR "/src/UserService/UserService"
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish

# ===== Final =====
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserService.dll"]
