# ================================================================
# deploy-azure.ps1
# Script de despliegue para Azure — Supermercado API
# Ejecutar desde la raíz del proyecto: .\deploy-azure.ps1
# ================================================================

param(
    [string]$ResourceGroup    = "supermercado-rg",
    [string]$Location         = "eastus",
    [string]$SqlServerName    = "supermercado-sqlserver",
    [string]$SqlAdminUser     = "sqladmin",
    [string]$SqlAdminPassword = "SuperMercado2024!",
    [string]$StorageName      = "supermercadostorage",
    [string]$AcrName          = "supermercadoacr",
    [string]$AppServicePlan   = "supermercado-plan",
    [string]$WebAppName       = "supermercado-api-app"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Supermercado API — Deploy a Azure    " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ----- PASO 1: Resource Group -----
Write-Host "[1/7] Creando Resource Group..." -ForegroundColor Yellow
az group create --name $ResourceGroup --location $Location
Write-Host "OK: Resource Group '$ResourceGroup' listo" -ForegroundColor Green
Write-Host ""

# ----- PASO 2: Azure SQL -----
Write-Host "[2/7] Creando Azure SQL Server y base de datos..." -ForegroundColor Yellow
az sql server create `
    --name $SqlServerName `
    --resource-group $ResourceGroup `
    --location $Location `
    --admin-user $SqlAdminUser `
    --admin-password $SqlAdminPassword

az sql db create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name SupermercadoDB `
    --service-objective Basic

az sql server firewall-rule create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

$SqlConnectionString = "Server=$SqlServerName.database.windows.net,1433;Database=SupermercadoDB;User Id=$SqlAdminUser;Password=$SqlAdminPassword;TrustServerCertificate=False;Encrypt=True;"
Write-Host "OK: SQL Server listo" -ForegroundColor Green
Write-Host ""

# ----- PASO 3: Azure Storage Queue -----
Write-Host "[3/7] Creando Storage Account para la cola de pedidos..." -ForegroundColor Yellow
az storage account create `
    --name $StorageName `
    --resource-group $ResourceGroup `
    --location $Location `
    --sku Standard_LRS

$StorageConnectionString = $(az storage account show-connection-string `
    --name $StorageName `
    --resource-group $ResourceGroup `
    --query connectionString `
    --output tsv)

Write-Host "OK: Storage Account '$StorageName' listo" -ForegroundColor Green
Write-Host "    Connection string guardada en variable" -ForegroundColor Gray
Write-Host ""

# ----- PASO 4: Azure Container Registry -----
Write-Host "[4/7] Creando Container Registry y subiendo imagen Docker..." -ForegroundColor Yellow
az acr create `
    --resource-group $ResourceGroup `
    --name $AcrName `
    --sku Basic `
    --admin-enabled true

az acr login --name $AcrName

# Build y push de la imagen
docker build `
    -f configs/docker/Dockerfile `
    -t "$AcrName.azurecr.io/supermercado-api:latest" `
    ./SuperMercado

docker push "$AcrName.azurecr.io/supermercado-api:latest"

$AcrUsername = $(az acr credential show --name $AcrName --query username -o tsv)
$AcrPassword = $(az acr credential show --name $AcrName --query "passwords[0].value" -o tsv)
Write-Host "OK: Imagen publicada en ACR" -ForegroundColor Green
Write-Host ""

# ----- PASO 5: App Service -----
Write-Host "[5/7] Creando Azure App Service..." -ForegroundColor Yellow
az appservice plan create `
    --name $AppServicePlan `
    --resource-group $ResourceGroup `
    --is-linux `
    --sku B1

az webapp create `
    --resource-group $ResourceGroup `
    --plan $AppServicePlan `
    --name $WebAppName `
    --deployment-container-image-name "$AcrName.azurecr.io/supermercado-api:latest" `
    --docker-registry-server-url "https://$AcrName.azurecr.io" `
    --docker-registry-server-user $AcrUsername `
    --docker-registry-server-password $AcrPassword

Write-Host "OK: App Service '$WebAppName' creado" -ForegroundColor Green
Write-Host ""

# ----- PASO 6: Variables de entorno -----
Write-Host "[6/7] Configurando variables de entorno en App Service..." -ForegroundColor Yellow
az webapp config appsettings set `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --settings `
        "ConnectionStrings__DefaultConnection=$SqlConnectionString" `
        "AzureStorage__ConnectionString=$StorageConnectionString" `
        "AzureStorage__QueueName=pedidos-supermercado" `
        "AzureStorage__UseInMemoryQueueWhenUnavailable=false" `
        "Email__Remitente=gsandovals@miumg.edu.gt" `
        "Email__Password=umer xios vmmk waqk" `
        "Email__SmtpHost=smtp.gmail.com" `
        "Email__SmtpPort=587" `
        "Jwt__Key=supermercado-jwt-key-produccion-2024-minimo-32-caracteres-largo" `
        "Jwt__Issuer=supermercado-api" `
        "Jwt__Audience=supermercado-app" `
        "Swagger__Enabled=true" `
        "ASPNETCORE_ENVIRONMENT=Production"

Write-Host "OK: Variables configuradas" -ForegroundColor Green
Write-Host ""

# ----- PASO 7: Verificar -----
Write-Host "[7/7] Verificando despliegue..." -ForegroundColor Yellow
Start-Sleep -Seconds 45

$AppUrl = $(az webapp show `
    --resource-group $ResourceGroup `
    --name $WebAppName `
    --query defaultHostName `
    --output tsv)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   DESPLIEGUE COMPLETADO                " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "API + Swagger:  https://$AppUrl/swagger" -ForegroundColor White
Write-Host "Health check:   https://$AppUrl/health" -ForegroundColor White
Write-Host ""
Write-Host "Siguiente paso:" -ForegroundColor Yellow
Write-Host "  Editar UI/scripts/api.js y cambiar la URL a:" -ForegroundColor Gray
Write-Host "  const API = `"https://$AppUrl/api`";" -ForegroundColor Gray
Write-Host ""
