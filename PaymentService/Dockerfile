# =========================
#       BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj
COPY PaymentService/*.csproj ./PaymentService/
RUN dotnet restore PaymentService/PaymentService.csproj

# Копируем весь проект
COPY . .

# Сборка
RUN dotnet publish PaymentService/PaymentService.csproj -c Release -o /app/publish



# =========================
#       RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PaymentService.dll"]
