# Guía de Docker - Supermercado API

## Requisitos previos

- Docker Desktop instalado
- Docker Compose (incluido en Docker Desktop)

## Construcción de la imagen

Para construir la imagen de Docker manualmente:

```bash
docker build -t supermercado-api:latest .
```

## Ejecución con Docker Compose (Recomendado)

La forma más sencilla de ejecutar la aplicación con la base de datos:

```bash
docker-compose up -d
```

Esto levantará:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **SQL Server**: localhost:1433

### Detener los contenedores

```bash
docker-compose down
```

### Detener y eliminar volúmenes (borrar datos)

```bash
docker-compose down -v
```

## Ejecución manual con Docker

Si prefieres ejecutar la API sin Docker Compose, primero levanta SQL Server:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=SuperMercado123!" \
  -p 1433:1433 \
  -d \
  --name sqlserver \
  mcr.microsoft.com/mssql/server:2022-latest
```

Luego ejecuta la API:

```bash
docker run -p 5000:80 \
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=SupermercadoDB;User Id=sa;Password=SuperMercado123!;TrustServerCertificate=True;" \
  -d \
  --name supermercado-api \
  supermercado-api:latest
```

## Variables de entorno

Puedes personalizar las siguientes variables al ejecutar:

- `ASPNETCORE_ENVIRONMENT`: Entorno de la aplicación (Development, Production, etc.)
- `ConnectionStrings__DefaultConnection`: Cadena de conexión a SQL Server
- `ASPNETCORE_URLS`: URLs en las que escucha la aplicación

## Solución de problemas

### La API no se conecta a SQL Server

- Verifica que SQL Server esté ejecutándose: `docker ps`
- Comprueba que la cadena de conexión sea correcta
- En Windows con Docker Desktop, usa `host.docker.internal` en lugar de `localhost`

### Puertos en uso

Si los puertos 5000 o 1433 ya están en uso, modifica el `docker-compose.yml`:

```yaml
ports:
  - "5005:80"  # Cambiar el primer número al puerto disponible
```

### Ver logs

```bash
# Ver logs de todos los servicios
docker-compose logs -f

# Ver logs solo de la API
docker-compose logs -f api

# Ver logs solo de SQL Server
docker-compose logs -f sqlserver
```

## Migración de base de datos

Las migraciones se ejecutan automáticamente al iniciar la aplicación. Si necesitas hacerlo manualmente:

```bash
docker exec supermercado-api dotnet ef database update
```

## Producción

Para un entorno de producción:

1. Usa una imagen base más segura sin los headers de desarrollo
2. Configura variables de entorno apropiadas
3. Usa una base de datos SQL Server administrada (Azure SQL Database, AWS RDS, etc.)
4. Implementa certificados SSL/TLS válidos
5. Configura un proxy inverso (Nginx, Traefik, etc.)


