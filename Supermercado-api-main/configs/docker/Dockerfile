# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["supermercado.API/supermercado.API.csproj", "supermercado.API/"]

# Restaurar dependencias
RUN dotnet restore "supermercado.API/supermercado.API.csproj"

# Copiar todo el código
COPY . .

# Compilar en Release
RUN dotnet build "supermercado.API/supermercado.API.csproj" -c Release -o /app/build

# Publicar
RUN dotnet publish "supermercado.API/supermercado.API.csproj" -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Instalar herramientas de EF Core para migraciones
RUN apt-get update && apt-get install -y \
    dos2unix \
    && rm -rf /var/lib/apt/lists/*

# Copiar archivos publicados
COPY --from=build /app/publish .

# Script de entrada
COPY --chown=app:app ./entrypoint.sh ./
RUN dos2unix ./entrypoint.sh && \
    chmod +x ./entrypoint.sh

# Usuario sin privilegios
RUN id -u app &>/dev/null || useradd -m -u 1000 app && chown -R app:app /app
USER app

# Exponer puertos
EXPOSE 80 443

# Variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD dotnet /app/supermercado.API.dll --version || exit 1

# Punto de entrada
ENTRYPOINT ["./entrypoint.sh"]
