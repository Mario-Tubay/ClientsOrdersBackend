# ==========================================
# 1. Build Stage
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ClientOrderService.API.csproj", "./"]
RUN dotnet restore "ClientOrderService.API.csproj"

# Copy source code and publish release build
COPY . .
RUN dotnet publish "ClientOrderService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# 2. Runtime Stage
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ClientOrderService.API.dll"]
