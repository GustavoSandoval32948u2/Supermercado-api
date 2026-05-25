# Script para automatizar comandos Docker en PowerShell
# Uso: .\docker-commands.ps1 [comando]
# Ejemplo: .\docker-commands.ps1 up

param(
    [Parameter(Position = 0)]
    [string]$Command = "help"
)

function Show-Help {
    Write-Host "=== Supermercado API - Comandos Docker ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Uso: .\docker-commands.ps1 [comando]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Comandos disponibles:" -ForegroundColor Green
    Write-Host "  up              - Levantar contenedores (desarrollo)"
    Write-Host "  up-prod         - Levantar contenedores (producción)"
    Write-Host "  down            - Detener contenedores"
    Write-Host "  down-clean      - Detener y eliminar volúmenes"
    Write-Host "  logs            - Ver logs en tiempo real"
    Write-Host "  logs-api        - Ver logs solo de la API"
    Write-Host "  logs-db         - Ver logs solo de SQL Server"
    Write-Host "  restart         - Reiniciar contenedores"
    Write-Host "  ps              - Listar contenedores"
    Write-Host "  build           - Construir imagen Docker"
    Write-Host "  clean           - Limpiar todo (CUIDADO: borra datos)"
    Write-Host "  status          - Ver estado del sistema"
    Write-Host "  db-shell        - Conectar a SQL Server"
    Write-Host "  api-shell       - Conectar a shell de la API"
    Write-Host ""
}

switch ($Command.ToLower()) {
    "up" {
        Write-Host "🚀 Levantando contenedores..." -ForegroundColor Green
        docker-compose up -d
        Write-Host "✅ Contenedores levantados" -ForegroundColor Green
        Write-Host "📍 API: http://localhost:5000" -ForegroundColor Cyan
        Write-Host "📊 Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
    }
    
    "up-prod" {
        Write-Host "🚀 Levantando contenedores (Producción)..." -ForegroundColor Green
        docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
        Write-Host "✅ Contenedores levantados" -ForegroundColor Green
    }
    
    "down" {
        Write-Host "⛔ Deteniendo contenedores..." -ForegroundColor Yellow
        docker-compose down
        Write-Host "✅ Contenedores detenidos" -ForegroundColor Green
    }
    
    "down-clean" {
        Write-Host "🗑️  Deteniendo y eliminando volúmenes..." -ForegroundColor Red
        $confirm = Read-Host "¿Estás seguro? (S/N)"
        if ($confirm -eq "S" -or $confirm -eq "s") {
            docker-compose down -v
            Write-Host "⚠️  Se eliminaron todos los datos" -ForegroundColor Red
        }
    }
    
    "logs" {
        docker-compose logs -f
    }
    
    "logs-api" {
        docker-compose logs -f api
    }
    
    "logs-db" {
        docker-compose logs -f sqlserver
    }
    
    "restart" {
        Write-Host "🔄 Reiniciando contenedores..." -ForegroundColor Yellow
        docker-compose restart
        Write-Host "✅ Contenedores reiniciados" -ForegroundColor Green
    }
    
    "ps" {
        docker-compose ps
    }
    
    "build" {
        Write-Host "🔨 Construyendo imagen Docker..." -ForegroundColor Green
        docker-compose build
    }
    
    "clean" {
        Write-Host "🧹 Limpiando sistema Docker..." -ForegroundColor Red
        $confirm = Read-Host "¿Estás seguro? Esto eliminará todo (S/N)"
        if ($confirm -eq "S" -or $confirm -eq "s") {
            docker-compose down -v
            docker system prune -a --volumes -f
            Write-Host "✅ Sistema limpiado" -ForegroundColor Green
        }
    }
    
    "status" {
        Write-Host "=== Estado de Contenedores ===" -ForegroundColor Cyan
        docker-compose ps
        Write-Host ""
        Write-Host "=== Estadísticas ===" -ForegroundColor Cyan
        docker stats --no-stream
    }
    
    "db-shell" {
        Write-Host "Conectando a SQL Server..." -ForegroundColor Yellow
        $password = if (Test-Path ".env.local") {
            (Get-Content ".env.local" | Select-String "SA_PASSWORD").ToString().Split("=")[1]
        } else {
            "SuperMercado123!"
        }
        docker exec -it supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $password
    }
    
    "api-shell" {
        Write-Host "Conectando a contenedor API..." -ForegroundColor Yellow
        docker exec -it supermercado-api /bin/bash
    }
    
    "help" {
        Show-Help
    }
    
    default {
        Write-Host "❌ Comando desconocido: $Command" -ForegroundColor Red
        Show-Help
    }
}
