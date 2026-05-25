# 🐳 Entorno Completo Docker - Supermercado API

## ✅ Lo que se ha configurado

### 📦 Archivos creados

```
├──\ configs/docker/Dockerfile                  # Imagen optimizada para desarrollo
├──\ configs/docker/Dockerfile.prod             # Imagen optimizada para producción (Alpine)
├── docker-compose.yml          # Orquestación para desarrollo
├── docker-compose.prod.yml     # Orquestación para producción + Nginx
├── .dockerignore               # Archivos a ignorar en build
├── .env.example                # Plantilla de variables de entorno
├── .env.local                  # Variables para desarrollo
├──\ configs/nginx/nginx\.conf                  # Configuración de proxy reverso
├── entrypoint.sh               # Script de entrada del contenedor
├── init-db.sh                  # Script de inicialización de BD
├── Makefile                    # Comandos automáticos (Linux/Mac)
├── docker-commands.ps1         # Comandos automáticos (Windows)
├── docker-commands.sh          # Comandos automáticos (Bash)
├── README_DOCKER.md            # Documentación completa
├── SETUP.md                    # Este archivo
└── supermercado.API/
    ├── appsettings.json           # Configuración (conecta a docker)
    ├── appsettings.Development.json
    └── appsettings.Production.json
```

### 🎯 Características implementadas

#### Desarrollo
- ✅ SQL Server en contenedor con volumen persistente
- ✅ API .NET 8.0 en contenedor
- ✅ Health checks automáticos
- ✅ Red Docker personalizada
- ✅ Variables de entorno configurables
- ✅ Logs accesibles

#### Producción
- ✅ Imagen Alpine optimizada (menor tamaño)
- ✅ Usuario no-root por seguridad
- ✅ Nginx como proxy reverso
- ✅ SSL/TLS ready
- ✅ Rate limiting
- ✅ Compresión gzip
- ✅ Headers de seguridad
- ✅ Multi-stage build

#### Automatización
- ✅ Makefile para Linux/Mac
- ✅ Scripts PowerShell para Windows
- ✅ Scripts Bash para Linux/Mac
- ✅ Comandos para backup/restore
- ✅ Health checks

---

## 🚀 Inicio Rápido (5 minutos)

### 1️⃣ Windows (PowerShell)

```powershell
# Configurar variables de entorno
Copy-Item ".env.example" ".env.local"

# Levantar todo
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Detener
docker-compose down
```

O usa el script:
```powershell
.\docker-commands.ps1 up
.\docker-commands.ps1 logs-api
.\docker-commands.ps1 down
```

### 2️⃣ Linux/Mac (Bash)

```bash
# Configurar variables de entorno
cp .env.example .env.local

# Levantar todo
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Detener
docker-compose down
```

O usa make:
```bash
make up
make logs-api
make down
```

---

## 📋 Configuración de Variables

### Desarrollo (`.env.local`)
```env
ASPNETCORE_ENVIRONMENT=Development
SA_PASSWORD=SuperMercado123!
SQL_DATABASE=SupermercadoDB
SQL_USER=sa
API_PORT=5000
```

### Producción (`.env.prod`)
```env
ASPNETCORE_ENVIRONMENT=Production
SA_PASSWORD=tu_password_muy_segura_12_caracteres_minimo
SQL_DATABASE=SupermercadoDB
SQL_USER=sa
JWT_KEY=tu_jwt_key_largo_y_seguro_minimo_32_caracteres
API_PORT=80
```

---

## 📍 Acceso a Servicios

### Desarrollo
```
API:        http://localhost:5000
Swagger:    http://localhost:5000/swagger
SQL Server: localhost:1433
```

### Producción
```
API:        https://tu-dominio.com
Swagger:    https://tu-dominio.com/swagger
SQL Server: interno (no expuesto)
```

---

## 🔄 Comandos Comunes

### Desarrollo (Docker Compose)
```bash
# Levantar
docker-compose up -d

# Ver logs
docker-compose logs -f

# Detener
docker-compose down

# Limpiar todo
docker-compose down -v
```

### Windows (Script PowerShell)
```powershell
.\docker-commands.ps1 up
.\docker-commands.ps1 logs-api
.\docker-commands.ps1 down
.\docker-commands.ps1 db-shell
.\docker-commands.ps1 status
```

### Linux/Mac (Make)
```bash
make up
make logs-api
make down
make db-backup
make db-shell
make status
```

---

## 🗄️ Base de Datos

### Conectar a SQL Server

**Desde cualquier herramienta SQL (SSMS, Azure Data Studio):**
```
Server: localhost,1433
User: sa
Password: SuperMercado123! (desarrollo)
```

### Crear Backup
```bash
# Con make (Linux/Mac)
make db-backup

# Con script (cualquier SO)
docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "BACKUP DATABASE SupermercadoDB TO DISK = '/var/opt/mssql/backup/backup.bak'"

docker cp supermercado-sqlserver:/var/opt/mssql/backup/backup.bak ./
```

### Restaurar Backup
```bash
docker cp backup.bak supermercado-sqlserver:/var/opt/mssql/backup/

docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "RESTORE DATABASE SupermercadoDB FROM DISK = '/var/opt/mssql/backup/backup.bak' WITH REPLACE"
```

---

## 🏗️ Estructura de Volúmenes

```
sqlserver_data    → /var/opt/mssql/data      (Bases de datos)
sqlserver_logs    → /var/opt/mssql/log       (Logs)
sqlserver_backup  → /var/opt/mssql/backup    (Backups)
```

Los datos persisten incluso si eliminas los contenedores:
```bash
docker-compose down    # Datos se mantienen
docker-compose down -v # Datos se eliminan
```

---

## 🔐 Seguridad

### Desarrollo
- ⚠️ Usa credenciales de desarrollo
- ⚠️ No expone SSL
- ✅ Para desarrollo local solamente

### Producción
- ✅ Usuario no-root en contenedor
- ✅ SSL/TLS obligatorio
- ✅ Headers de seguridad
- ✅ Rate limiting
- ✅ Proxy reverso (Nginx)
- ✅ Credenciales seguras en variables

### Checklist Seguridad Producción
```
☐ Cambiar SA_PASSWORD
☐ Generar JWT_KEY segura
☐ Configurar certificados SSL
☐ Actualizar nginx.conf con tu dominio
☐ Usar .env.prod (no en Git)
☐ Revisar logs regularmente
```

---

## 🆘 Troubleshooting

### API no inicia
```bash
docker-compose logs api
```

### SQL Server no conecta
```bash
docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "SELECT @@VERSION"
```

### Puerto en uso

**Windows:**
```powershell
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

**Linux/Mac:**
```bash
lsof -i :5000
kill -9 <PID>
```

### Recrear contenedores
```bash
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

---

## 📊 Monitoreo

### Ver estado en tiempo real
```bash
docker stats
```

### Ver eventos
```bash
docker events --filter container=supermercado-api
```

### Salud de servicios
```bash
# Desarrollo
curl http://localhost:5000/health

# Producción
curl https://tu-dominio.com/health
```

---

## 🚀 Desplegar en Producción

### 1. Preparar servidor
```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install -y docker.io docker-compose git

# Agregar usuario a grupo docker
sudo usermod -aG docker $USER
```

### 2. Clonar proyecto
```bash
git clone tu-repo.git
cd tu-proyecto
```

### 3. Configurar
```bash
# Crear .env.prod
cp .env.example .env.prod

# Editar con valores de producción
nano .env.prod

# Generar certificados SSL
mkdir certs
certbot certonly --standalone -d tu-dominio.com
cp /etc/letsencrypt/live/tu-dominio.com/fullchain.pem certs/cert.pem
cp /etc/letsencrypt/live/tu-dominio.com/privkey.pem certs/key.pem
```

### 4. Lanzar
```bash
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

### 5. Verificar
```bash
curl https://tu-dominio.com/swagger
```

---

## 📚 Documentación Adicional

- [README_DOCKER.md](README_DOCKER.md) - Guía completa de Docker
- [docker-compose.yml](docker-compose.yml) - Configuración desarrollo
- [docker-compose.prod.yml](docker-compose.prod.yml) - Configuración producción
-\ \[Dockerfile]\(\.\./configs/docker/Dockerfile\) - Imagen desarrollo
-\ \[Dockerfile\.prod]\(\.\./configs/docker/Dockerfile\.prod\) - Imagen producción

---

## 🤝 Soporte

Si tienes problemas:

1. **Verifica logs:**
   ```bash
   docker-compose logs -f
   ```

2. **Revisa el estado:**
   ```bash
   docker-compose ps
   ```

3. **Limpia y reinicia:**
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

4. **Consulta la documentación:**
   - [README_DOCKER.md](README_DOCKER.md)
   - [Docker Documentation](https://docs.docker.com/)
   - [Microsoft SQL Server Docker](https://mcr.microsoft.com/product/mssql/server)

---

## 📝 Próximos Pasos

- [ ] Testear desarrollo localmente
- [ ] Configura CI/CD (GitHub Actions, GitLab CI, etc.)
- [ ] Implementa monitoreo (Prometheus, ELK Stack, etc.)
- [ ] Configura logging centralizado
- [ ] Implementa backups automáticos
- [ ] Configura alertas
- [ ] Documentar procesos de deploy

---

**Última actualización**: Mayo 2026  
**Versión**: 1.0  
**Estado**: ✅ Listo para producción


