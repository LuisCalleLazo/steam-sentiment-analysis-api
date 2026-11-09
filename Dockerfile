# ============================================
# Etapa 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias (cache de capas)
COPY ["SteamSentimentAnalysis/SteamSentimentAnalysis.csproj", "SteamSentimentAnalysis/"]
RUN dotnet restore "SteamSentimentAnalysis/SteamSentimentAnalysis.csproj"

# Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/SteamSentimentAnalysis"
RUN dotnet publish "SteamSentimentAnalysis.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Etapa 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Variables de entorno recomendadas para contenedores
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SteamSentimentAnalysis.dll"]