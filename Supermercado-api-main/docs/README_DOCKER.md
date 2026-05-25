# Supermercado API - Guía Completa de Configuración

## 📋 Requisitos

- Docker Desktop instalado (incluye Docker y Docker Compose)
- Git
- Espacio en disco: ~5GB mínimo

## 🚀 Inicio Rápido

### 1. Clonar y configurar

```bash
cd tu-proyecto
cp .env.example .env.local
```

### 2. Levantar todo (Desarrollo)

```bash
docker-compose up -d
```

La API estará disponible en:
- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **SQL Server**: localhost:1433

### 3. Verificar estado

```bash
# Ver estado de contenedores
docker-compose ps

# Ver logs
docker-compose logs -f api
docker-compose logs -f sqlserver

# Health check
curl http://localhost:5000/health
```

## 🛑 Detener

```bash
# Detener sin eliminar volúmenes (datos persistentes)
docker-compose down

# Detener y eliminar volúmenes (borra toda la BD)
docker-compose down -v
```

## 📁 Estructura de Archivos

```
├──\ configs/docker/Dockerfile              # Imagen de la API
├── docker-compose.yml      # Orquestación desarrollo
├── docker-compose.prod.yml # Orquestación producción
├── .env.example           # Variables de ejemplo
├── .env.local             # Variables de desarrollo (no subir a Git)
├── .dockerignore          # Archivos a ignorar en build
├── init-db.sh            # Script de inicialización DB
├── entrypoint.sh         # Script de entrada del contenedor
├──\ configs/nginx/nginx\.conf            # Configuración reverse proxy
└── supermercado.API/
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── appsettings.Production.json
    └── ...
```

## 🌍 Variables de Entorno

Ver `.env.example` para todas las opciones. Las más importantes:

```env
ASPNETCORE_ENVIRONMENT=Development
SA_PASSWORD=SuperMercado123!
SQL_DATABASE=SupermercadoDB
SQL_USER=sa
API_PORT=5000
```

## 🔧 Configuración Personalizada

### Cambiar puertos

Edita `.env.local`:
```env
API_PORT=8000
SQL_PORT=1434
```

Luego recrea los contenedores:
```bash
docker-compose down
docker-compose up -d
```

### Cambiar credenciales SQL Server

En `.env.local`:
```env
SA_PASSWORD=tu_password_segura
```

**IMPORTANTE**: No uses estas credenciales en producción.

## 🗄️ Base de Datos

### Conectar a SQL Server

```bash
# Desde la host (Windows, Linux, Mac)
sqlcmd -S localhost,1433 -U sa -P SuperMercado123!

# O usar Azure Data Studio
# Server: localhost,1433
# Username: sa
# Password: SuperMercado123!
```

### Crear backup

```bash
docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "BACKUP DATABASE SupermercadoDB TO DISK = '/var/opt/mssql/backup/backup.bak'"

# Copiar archivo de backup a host
docker cp supermercado-sqlserver:/var/opt/mssql/backup/backup.bak ./
```

### Restaurar backup

```bash
# Copiar archivo al contenedor
docker cp backup.bak supermercado-sqlserver:/var/opt/mssql/backup/

# Restaurar
docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "RESTORE DATABASE SupermercadoDB FROM DISK = '/var/opt/mssql/backup/backup.bak' WITH REPLACE"
```

## 🏗️ Producción

### Preparar

1. Crea un `.env.prod`:

```env
ASPNETCORE_ENVIRONMENT=Production
SA_PASSWORD=tu_password_muy_segura_min_12_caracteres
SQL_DATABASE=SupermercadoDB
SQL_USER=sa
JWT_KEY=tu_jwt_key_muy_largo_y_seguro
```

2. Configura certificados SSL en `./certs/`:
   - `cert.pem`
   - `key.pem`

O genera con Let's Encrypt:
```bash
docker run -it --rm -v $(pwd)/certs:/etc/letsencrypt certbot certbot certonly \
  --standalone -d tu-dominio.com
```

3. Actualiza `nginx.conf` con tu dominio

### Lanzar producción

```bash
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

## 🔍 Troubleshooting

### API no inicia
```bash
# Ver logs
docker-compose logs api

# Verificar que DB esté lista
docker-compose logs sqlserver
```

### Conexión a SQL Server fallida

```bash
# Ejecutar comando test en SQL Server
docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "SELECT @@VERSION"
```

### Puerto en uso

```bash
# Windows
netstat -ano | findstr :5000

# Linux/Mac
lsof -i :5000

# Liberar puerto o cambiar en .env.local
```

### Limpiar todo (cuidado: borra datos)

```bash
docker-compose down -v
docker system prune -a --volumes
```

## 📊 Monitoreo

### Ver estadísticas en tiempo real

```bash
docker stats
```

### Ver eventos

```bash
docker events --filter container=supermercado-api
```

## 🔐 Seguridad

- ✅ Usa variables de entorno para secretos
- ✅ Nunca subas `.env.local` a Git
- ✅ Cambia las credenciales por defecto en producción
- ✅ Usa HTTPS con certificados válidos
- ✅ Mantén Docker Desktop actualizado
- ✅ Revisa logs regularmente

## 📝 Logs

```bash
# Todos los logs
docker-compose logs

# Últimas 100 líneas
docker-compose logs --tail=100

# En tiempo real
docker-compose logs -f

# Solo de un servicio
docker-compose logs -f api
```

## 🆘 Soporte

Para problemas:
1. Revisa los logs: `docker-compose logs -f`
2. Verifica que Docker esté corriendo
3. Comprueba que los puertos no estén en uso
4. Asegúrate de tener espacio en disco

---

**Última actualización**: Mayo 2026


