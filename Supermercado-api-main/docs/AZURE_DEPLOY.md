# ☁️ Despliegue en Azure - Supermercado API

Esta guía explica paso a paso cómo mover todo el proyecto de localhost a Azure real.
No se modifica ningún archivo del código — solo se configuran variables de entorno en el portal.

---

## ¿Qué se va a crear en Azure?

| Recurso | Para qué sirve |
|---|---|
| **Resource Group** | Contenedor lógico para todos los recursos |
| **Azure SQL Server + Database** | Reemplaza el SQL Server local de Docker |
| **Azure Storage Account + Queue** | Reemplaza Azurite local (la cola de pedidos) |
| **Azure Container Registry (ACR)** | Guarda la imagen Docker de la API |
| **Azure App Service** | Corre la API en la nube |
| **Azure Static Web Apps** | Publica la UI (HTML/JS) |

Tiempo estimado: 45-60 minutos la primera vez.

---

## PASO 1 — Crear el Resource Group

Abrir la terminal con Azure CLI instalado (o usar Azure Cloud Shell en portal.azure.com).

```bash
# Iniciar sesión
az login

# Crear grupo de recursos (cambia la región si quieres)
az group create \
  --name supermercado-rg \
  --location eastus
```

> **Nota:** Usar siempre el mismo `--resource-group supermercado-rg` en todos los comandos siguientes.

---

## PASO 2 — Crear Azure SQL Server y Base de Datos

```bash
# Crear el servidor SQL (cambia los valores entre <> por los tuyos)
az sql server create \
  --name supermercado-sqlserver \
  --resource-group supermercado-rg \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "SuperMercado2024!"

# Crear la base de datos (Basic tier = gratis los primeros días)
az sql db create \
  --resource-group supermercado-rg \
  --server supermercado-sqlserver \
  --name SupermercadoDB \
  --service-objective Basic

# Permitir que Azure App Service se conecte (regla de firewall)
az sql server firewall-rule create \
  --resource-group supermercado-rg \
  --server supermercado-sqlserver \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

Guardar la connection string (se usa en el Paso 5):
```
Server=supermercado-sqlserver.database.windows.net,1433;Database=SupermercadoDB;User Id=sqladmin;Password=SuperMercado2024!;TrustServerCertificate=False;Encrypt=True;
```

---

## PASO 3 — Crear Azure Storage Account y la Cola

```bash
# Crear el Storage Account (nombre debe ser único en Azure, todo minúsculas)
az storage account create \
  --name supermercadostorage \
  --resource-group supermercado-rg \
  --location eastus \
  --sku Standard_LRS

# Obtener la connection string del storage
az storage account show-connection-string \
  --name supermercadostorage \
  --resource-group supermercado-rg \
  --query connectionString \
  --output tsv
```

Copiar el valor que devuelve ese comando — empieza con `DefaultEndpointsProtocol=https;AccountName=supermercadostorage;...`

> La cola `pedidos-supermercado` se crea sola la primera vez que la API arranca. No hay que crearla manualmente.

---

## PASO 4 — Crear Azure Container Registry y subir la imagen Docker

```bash
# Crear el registro de contenedores
az acr create \
  --resource-group supermercado-rg \
  --name supermercadoacr \
  --sku Basic \
  --admin-enabled true

# Iniciar sesión en el registro
az acr login --name supermercadoacr

# Ir a la raíz del proyecto (donde está el docker-compose.yml)
cd Supermercado-api-main

# Construir y etiquetar la imagen
docker build \
  -f configs/docker/Dockerfile \
  -t supermercadoacr.azurecr.io/supermercado-api:latest \
  -t supermercadoacr.azurecr.io/supermercado-api:v1.0 \
  ./SuperMercado

# Subir la imagen al registro
docker push supermercadoacr.azurecr.io/supermercado-api:latest
docker push supermercadoacr.azurecr.io/supermercado-api:v1.0
```

---

## PASO 5 — Crear Azure App Service y configurar variables

```bash
# Crear plan de App Service (B1 = nivel básico, el más económico con contenedores)
az appservice plan create \
  --name supermercado-plan \
  --resource-group supermercado-rg \
  --is-linux \
  --sku B1

# Obtener credenciales del ACR para el App Service
ACR_USERNAME=$(az acr credential show --name supermercadoacr --query username -o tsv)
ACR_PASSWORD=$(az acr credential show --name supermercadoacr --query passwords[0].value -o tsv)

# Crear la Web App con la imagen del registro
az webapp create \
  --resource-group supermercado-rg \
  --plan supermercado-plan \
  --name supermercado-api-app \
  --deployment-container-image-name supermercadoacr.azurecr.io/supermercado-api:latest \
  --docker-registry-server-url https://supermercadoacr.azurecr.io \
  --docker-registry-server-user $ACR_USERNAME \
  --docker-registry-server-password $ACR_PASSWORD
```

Ahora configurar las variables de entorno (reemplazando los valores locales):

```bash
# Pegar la connection string del Paso 2 y el storage del Paso 3
az webapp config appsettings set \
  --resource-group supermercado-rg \
  --name supermercado-api-app \
  --settings \
    "ConnectionStrings__DefaultConnection=Server=supermercado-sqlserver.database.windows.net,1433;Database=SupermercadoDB;User Id=sqladmin;Password=SuperMercado2024!;TrustServerCertificate=False;Encrypt=True;" \
    "AzureStorage__ConnectionString=DefaultEndpointsProtocol=https;AccountName=supermercadostorage;AccountKey=TU_ACCOUNT_KEY;EndpointSuffix=core.windows.net" \
    "AzureStorage__QueueName=pedidos-supermercado" \
    "AzureStorage__UseInMemoryQueueWhenUnavailable=false" \
    "Email__Remitente=gsandovals@miumg.edu.gt" \
    "Email__Password=umer xios vmmk waqk" \
    "Email__SmtpHost=smtp.gmail.com" \
    "Email__SmtpPort=587" \
    "Jwt__Key=supermercado-jwt-key-produccion-2024-minimo-32-caracteres" \
    "Jwt__Issuer=supermercado-api" \
    "Jwt__Audience=supermercado-app" \
    "Swagger__Enabled=true" \
    "ASPNETCORE_ENVIRONMENT=Production"
```

Verificar que la API está corriendo:
```bash
# Ver la URL asignada
az webapp show \
  --resource-group supermercado-rg \
  --name supermercado-api-app \
  --query defaultHostName \
  --output tsv
```

La URL quedará como: `https://supermercado-api-app.azurewebsites.net`

---

## PASO 6 — Ejecutar las migraciones de base de datos

Las migraciones se aplican automáticamente cuando la API arranca (el `entrypoint.sh` ya lo hace), pero si se quiere forzar manualmente:

```bash
# Acceder a la consola del App Service
az webapp ssh \
  --resource-group supermercado-rg \
  --name supermercado-api-app

# Dentro de la consola:
cd /app
dotnet ef database update \
  --connection "Server=supermercado-sqlserver.database.windows.net,1433;Database=SupermercadoDB;User Id=sqladmin;Password=SuperMercado2024!;TrustServerCertificate=False;Encrypt=True;"
```

---

## PASO 7 — Publicar la UI apuntando a Azure

Editar el archivo `UI/scripts/api.js` cambiando la URL local por la de Azure:

```js
// ANTES (localhost)
const API = "http://localhost:5143/api";

// DESPUÉS (Azure)
const API = "https://supermercado-api-app.azurewebsites.net/api";
```

Luego desplegar la UI en Azure Static Web Apps:

```bash
# Crear el Static Web App (la UI es HTML puro, no necesita build)
az staticwebapp create \
  --name supermercado-ui \
  --resource-group supermercado-rg \
  --location eastus2 \
  --source https://github.com/TU_USUARIO/TU_REPO \
  --branch main \
  --app-location "/UI" \
  --login-with-github
```

> Si no se quiere usar GitHub Actions, se puede subir manualmente con la CLI de Static Web Apps o simplemente alojar los archivos HTML en el App Service dentro de una carpeta `wwwroot`.

---

## PASO 8 — Verificar que Swagger funciona en Azure

Abrir en el navegador:
```
https://supermercado-api-app.azurewebsites.net/swagger
```

Si aparece la interfaz de Swagger con todos los endpoints, el despliegue está completo.

Para verificar la cola y el flujo completo de pedidos → email:

1. En Swagger, ir a `POST /api/PedidosQueue/simular-pedido-completo`
2. Usar este body:
```json
{
  "ventaId": 1,
  "clienteNombre": "Cliente Prueba",
  "emailDestino": "tucorreo@gmail.com",
  "total": 150.00
}
```
3. Revisar el correo — deben llegar 3 notificaciones (Recibido, Despachado, Entregado)

---

## Resumen de URLs finales

| Recurso | URL |
|---|---|
| API + Swagger | `https://supermercado-api-app.azurewebsites.net/swagger` |
| Health check | `https://supermercado-api-app.azurewebsites.net/health` |
| UI | `https://supermercado-ui.azurestaticapps.net` |
| Azure Portal | `https://portal.azure.com` → supermercado-rg |

---

## Solución de problemas comunes

**La API no arranca (500 Internal Server Error)**
```bash
# Ver los logs en tiempo real
az webapp log tail \
  --resource-group supermercado-rg \
  --name supermercado-api-app
```

**La cola no funciona (error de conexión)**
Verificar que `AzureStorage__ConnectionString` tenga el valor real del portal, no `UseDevelopmentStorage=true`.

**La UI no se conecta a la API (CORS error)**
El `Program.cs` ya tiene `AllowAnyOrigin()` configurado, así que no debería haber problemas. Verificar que `api.js` apunte a la URL correcta de Azure.

**La base de datos no tiene tablas**
Las migraciones de EF Core se aplican en el `entrypoint.sh`. Si hay un error, revisar que la connection string incluya `TrustServerCertificate=False;Encrypt=True;` para Azure SQL (diferente al local).
