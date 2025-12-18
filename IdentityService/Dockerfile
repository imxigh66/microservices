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

COPY ["IdentityService/IdentityService/IdentityService.csproj", "IdentityService/"]

RUN dotnet restore "IdentityService/IdentityService.csproj"

COPY IdentityService/IdentityService/. IdentityService/

WORKDIR "/src/IdentityService"
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.dll"]
