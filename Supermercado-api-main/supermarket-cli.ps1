# PowerShell equivalent del CLI maestro

param(
    [Parameter(Position = 0)]
    [string]$Command = "help",
    
    [Parameter(Position = 1)]
    [int]$Version = 0
)

$ErrorActionPreference = "Stop"

function Write-Banner {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                                                            ║" -ForegroundColor Cyan
    Write-Host "║          🏪 SUPERMERCADO API - CLI MAESTRO v2.0             ║" -ForegroundColor Cyan
    Write-Host "║                                                            ║" -ForegroundColor Cyan
    Write-Host "║   Gestión centralizada de todas las versiones             ║" -ForegroundColor Cyan
    Write-Host "║                                                            ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Show-Menu {
    Write-Host "┌─ VERSIONES DISPONIBLES ─────────────────────────┐" -ForegroundColor White
    Write-Host "1. Desarrollo Simple      (1 API)" -ForegroundColor Green
    Write-Host "2. Microservicios        (2-N APIs + Load Balancer)" -ForegroundColor Yellow
    Write-Host "3. Kubernetes             (Auto-scaling)" -ForegroundColor Red
    Write-Host "4. Producción             (Nginx + Reverse Proxy)" -ForegroundColor Blue
    Write-Host "└──────────────────────────────────────────────────┘" -ForegroundColor White
    Write-Host ""
    Write-Host "┌─ COMANDOS ────────────────────────────────────────┐" -ForegroundColor White
    Write-Host "up              Levantar ambiente seleccionado" -ForegroundColor Green
    Write-Host "down            Detener ambiente" -ForegroundColor Red
    Write-Host "logs            Ver logs" -ForegroundColor Yellow
    Write-Host "scale           Escalar instancias" -ForegroundColor Blue
    Write-Host "status          Ver estado" -ForegroundColor Green
    Write-Host "test            Ejecutar tests" -ForegroundColor Cyan
    Write-Host "build           Compilar imágenes" -ForegroundColor Yellow
    Write-Host "clean           Limpiar todo" -ForegroundColor Red
    Write-Host "help            Ver ayuda" -ForegroundColor Blue
    Write-Host "└──────────────────────────────────────────────────┘" -ForegroundColor White
}

function Select-Version {
    param([int]$VersionNum = 0)
    
    if ($VersionNum -eq 0) {
        Write-Host "Selecciona versión:" -ForegroundColor Yellow
        [int]$VersionNum = Read-Host "1-4"
    }
    
    switch ($VersionNum) {
        1 { 
            $script:VersionName = "Desarrollo"
            $script:ComposeFile = "docker-compose.yml"
        }
        2 { 
            $script:VersionName = "Microservicios"
            $script:ComposeFile = "docker-compose.microservices.yml"
        }
        3 { 
            $script:VersionName = "Kubernetes"
            $script:ComposeFile = "kubernetes-deployment.yml"
        }
        4 { 
            $script:VersionName = "Producción"
            $script:ComposeFile = "docker-compose.prod.yml"
        }
        default {
            Write-Host "❌ Opción inválida" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "✓ Versión seleccionada: $VersionName" -ForegroundColor Green
}

function Version-Up {
    param([int]$VersionNum)
    
    Select-Version $VersionNum
    
    Write-Host "🚀 Levantando $VersionName..." -ForegroundColor Yellow
    
    if ($ComposeFile -eq "kubernetes-deployment.yml") {
        & ./deploy-k8s.sh
    } else {
        docker-compose -f $ComposeFile up -d
    }
    
    Write-Host "✅ $VersionName levantado" -ForegroundColor Green
    Print-AccessInfo $VersionNum
}

function Version-Down {
    param([int]$VersionNum)
    
    Select-Version $VersionNum
    
    Write-Host "⛔ Deteniendo $VersionName..." -ForegroundColor Yellow
    
    if ($ComposeFile -eq "kubernetes-deployment.yml") {
        kubectl delete namespace supermercado
    } else {
        docker-compose -f $ComposeFile down
    }
    
    Write-Host "✅ $VersionName detenido" -ForegroundColor Green
}

function Version-Logs {
    param([int]$VersionNum)
    
    Select-Version $VersionNum
    
    Write-Host "📋 Logs de $VersionName" -ForegroundColor Yellow
    
    if ($ComposeFile -eq "kubernetes-deployment.yml") {
        kubectl logs -f deployment/supermercado-api -n supermercado
    } else {
        docker-compose -f $ComposeFile logs -f
    }
}

function Version-Status {
    param([int]$VersionNum)
    
    Select-Version $VersionNum
    
    Write-Host "📊 Estado de $VersionName" -ForegroundColor Blue
    Write-Host ""
    
    if ($ComposeFile -eq "kubernetes-deployment.yml") {
        Write-Host "Pods:" -ForegroundColor Cyan
        kubectl get pods -n supermercado
        Write-Host ""
        Write-Host "Servicios:" -ForegroundColor Cyan
        kubectl get svc -n supermercado
    } else {
        docker-compose -f $ComposeFile ps
        Write-Host ""
        Write-Host "Estadísticas:" -ForegroundColor Cyan
        docker stats --no-stream
    }
}

function Print-AccessInfo {
    param([int]$VersionNum)
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Blue
    
    switch ($VersionNum) {
        1 {
            Write-Host "API: http://localhost:5000" -ForegroundColor Green
            Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Green
            Write-Host "SQL Server: localhost:1433" -ForegroundColor Green
        }
        2 {
            Write-Host "API (Load Balancer): http://localhost:5000" -ForegroundColor Green
            Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Green
            Write-Host "SQL Server: localhost:1433" -ForegroundColor Green
            Write-Host "Instancias: api-1, api-2 (+ más si escalaste)" -ForegroundColor Yellow
        }
        3 {
            Write-Host "Acceso: kubectl port-forward svc/supermercado-api-service 5000:80 -n supermercado" -ForegroundColor Green
            Write-Host "API: http://localhost:5000" -ForegroundColor Green
            Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Green
            Write-Host "Replicas: 2-10 (auto-scaling)" -ForegroundColor Yellow
        }
        4 {
            Write-Host "API (Nginx Proxy): http://localhost:5000" -ForegroundColor Green
            Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Green
            Write-Host "SSL/TLS: Configurar certificados en certs/" -ForegroundColor Yellow
        }
    }
    
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Blue
}

function Show-Help {
    Write-Banner
    Show-Menu
    Write-Host ""
    Write-Host "EJEMPLOS DE USO:" -ForegroundColor White
    Write-Host ""
    Write-Host "  # Desarrollo" -ForegroundColor Cyan
    Write-Host "  .\supermarket-cli.ps1 up 1"
    Write-Host "  .\supermarket-cli.ps1 logs 1"
    Write-Host "  .\supermarket-cli.ps1 down 1"
    Write-Host ""
    Write-Host "  # Microservicios" -ForegroundColor Cyan
    Write-Host "  .\supermarket-cli.ps1 up 2"
    Write-Host "  .\supermarket-cli.ps1 scale 2"
    Write-Host ""
    Write-Host "  # Kubernetes" -ForegroundColor Cyan
    Write-Host "  .\supermarket-cli.ps1 up 3"
    Write-Host "  .\supermarket-cli.ps1 scale 3"
    Write-Host ""
}

# Main
Write-Banner

switch ($Command.ToLower()) {
    "up" {
        Version-Up $Version
    }
    "down" {
        Version-Down $Version
    }
    "logs" {
        Version-Logs $Version
    }
    "status" {
        Version-Status $Version
    }
    "help" {
        Show-Help
    }
    default {
        Write-Host "❌ Comando desconocido: $Command" -ForegroundColor Red
        Show-Help
    }
}
