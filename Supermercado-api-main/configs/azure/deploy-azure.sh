#!/bin/bash
# ================================================================
# deploy-azure.sh
# Script de despliegue para Azure — Supermercado API
# Ejecutar desde la raíz del proyecto: bash configs/azure/deploy-azure.sh
# ================================================================

set -e  # detener si cualquier comando falla

RESOURCE_GROUP="supermercado-rg"
LOCATION="eastus"
SQL_SERVER="supermercado-sqlserver"
SQL_ADMIN="sqladmin"
SQL_PASSWORD="SuperMercado2024!"
STORAGE_NAME="supermercadostorage"
ACR_NAME="supermercadoacr"
APP_PLAN="supermercado-plan"
WEB_APP="supermercado-api-app"

echo "========================================"
echo "   Supermercado API — Deploy a Azure    "
echo "========================================"
echo ""

# ----- PASO 1: Resource Group -----
echo "[1/7] Creando Resource Group..."
az group create --name $RESOURCE_GROUP --location $LOCATION
echo "OK"
echo ""

# ----- PASO 2: Azure SQL -----
echo "[2/7] Creando Azure SQL Server y base de datos..."
az sql server create \
    --name $SQL_SERVER \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $SQL_ADMIN \
    --admin-password $SQL_PASSWORD

az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER \
    --name SupermercadoDB \
    --service-objective Basic

az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

SQL_CONN="Server=$SQL_SERVER.database.windows.net,1433;Database=SupermercadoDB;User Id=$SQL_ADMIN;Password=$SQL_PASSWORD;TrustServerCertificate=False;Encrypt=True;"
echo "OK"
echo ""

# ----- PASO 3: Azure Storage Queue -----
echo "[3/7] Creando Storage Account..."
az storage account create \
    --name $STORAGE_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku Standard_LRS

STORAGE_CONN=$(az storage account show-connection-string \
    --name $STORAGE_NAME \
    --resource-group $RESOURCE_GROUP \
    --query connectionString \
    --output tsv)
echo "OK"
echo ""

# ----- PASO 4: Container Registry -----
echo "[4/7] Creando Container Registry y subiendo imagen..."
az acr create \
    --resource-group $RESOURCE_GROUP \
    --name $ACR_NAME \
    --sku Basic \
    --admin-enabled true

az acr login --name $ACR_NAME

docker build \
    -f configs/docker/Dockerfile \
    -t $ACR_NAME.azurecr.io/supermercado-api:latest \
    ./SuperMercado

docker push $ACR_NAME.azurecr.io/supermercado-api:latest

ACR_USER=$(az acr credential show --name $ACR_NAME --query username -o tsv)
ACR_PASS=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)
echo "OK"
echo ""

# ----- PASO 5: App Service -----
echo "[5/7] Creando Azure App Service..."
az appservice plan create \
    --name $APP_PLAN \
    --resource-group $RESOURCE_GROUP \
    --is-linux \
    --sku B1

az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_PLAN \
    --name $WEB_APP \
    --deployment-container-image-name $ACR_NAME.azurecr.io/supermercado-api:latest \
    --docker-registry-server-url https://$ACR_NAME.azurecr.io \
    --docker-registry-server-user $ACR_USER \
    --docker-registry-server-password $ACR_PASS
echo "OK"
echo ""

# ----- PASO 6: Variables de entorno -----
echo "[6/7] Configurando variables en App Service..."
az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP \
    --settings \
        "ConnectionStrings__DefaultConnection=$SQL_CONN" \
        "AzureStorage__ConnectionString=$STORAGE_CONN" \
        "AzureStorage__QueueName=pedidos-supermercado" \
        "AzureStorage__UseInMemoryQueueWhenUnavailable=false" \
        "Email__Remitente=gsandovals@miumg.edu.gt" \
        "Email__Password=umer xios vmmk waqk" \
        "Email__SmtpHost=smtp.gmail.com" \
        "Email__SmtpPort=587" \
        "Jwt__Key=supermercado-jwt-key-produccion-2024-minimo-32-caracteres-largo" \
        "Jwt__Issuer=supermercado-api" \
        "Jwt__Audience=supermercado-app" \
        "Swagger__Enabled=true" \
        "ASPNETCORE_ENVIRONMENT=Production"
echo "OK"
echo ""

# ----- PASO 7: Verificar -----
echo "[7/7] Esperando que la app arranque..."
sleep 45

APP_URL=$(az webapp show \
    --resource-group $RESOURCE_GROUP \
    --name $WEB_APP \
    --query defaultHostName \
    --output tsv)

echo ""
echo "========================================"
echo "   DESPLIEGUE COMPLETADO"
echo "========================================"
echo ""
echo "API + Swagger:  https://$APP_URL/swagger"
echo "Health check:   https://$APP_URL/health"
echo ""
echo "Siguiente paso:"
echo "  Editar UI/scripts/api.js:"
echo "  const API = \"https://$APP_URL/api\";"
echo ""
