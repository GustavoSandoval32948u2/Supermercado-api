# 🏪 Supermercado API - Guía Maestra v2.0

> **Toda la información que necesitas en un solo lugar**

## 🎯 ¿Por Dónde Empiezo?

### ⚡ **Inicio en 5 Minutos**

```bash
# 1. Preparar
cp .env.example .env.local

# 2. Levantar (elige una línea)
./supermarket-cli.sh up 1        # Desarrollo (recomendado)
# O:
docker-compose up -d
```

### ✅ **Validar**
```bash
curl http://localhost:5000/swagger
```

---

## 🎛️ CLI Maestro - Tu Mejor Amigo

El **CLI Maestro** gestiona todas las 4 versiones desde un comando:

```bash
# Linux/Mac
./supermarket-cli.sh help
./supermarket-cli.sh up 1          # ← Empieza aquí
./supermarket-cli.sh logs 1
./supermarket-cli.sh down 1

# Windows PowerShell
.\supermarket-cli.ps1 help
.\supermarket-cli.ps1 up 1
.\supermarket-cli.ps1 logs 1
.\supermarket-cli.ps1 down 1
```

---

## 📋 Las 4 Versiones

```
DESARROLLO (1 API)
└─ docker-compose up -d
   2 min | http://localhost:5000

MICROSERVICIOS (2+ APIs + Load Balancer)
└─ docker-compose -f docker-compose.microservices.yml up -d
   5 min | Load balancing incluido

KUBERNETES (Auto-scaling 2-10)
└─ ./deploy-k8s.sh
   20 min | Producción real

PRODUCCIÓN (Nginx + SSL)
└─ docker-compose -f docker-compose.prod.yml up -d
   30 min | HTTPS incluido
```

**👉 Comienza con DESARROLLO si es tu primer día**

---

## 📚 DOCUMENTACIÓN RÁPIDA

| Necesidad | Documento | Tiempo |
|-----------|-----------|--------|
| **Empezar ya** | [SETUP.md](SETUP.md) | 5 min |
| **Entender todo** | [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) | 15 min |
| **Arquitecturas** | [ARCHITECTURE.md](ARCHITECTURE.md) | 10 min |
| **Testing** | [TESTING_GUIDE.md](TESTING_GUIDE.md) | 45 min |
| **Referencia** | [FILES_INDEX.md](FILES_INDEX.md) | lookup |

---

## 🚀 Roadmap de 4 Días

### Día 1️⃣: Desarrollo Local
```bash
./supermarket-cli.sh up 1
# Lee: SETUP.md (5 min)
# Testea: http://localhost:5000/swagger
```

### Día 2️⃣: Múltiples Instancias
```bash
./supermarket-cli.sh up 2
# Lee: CONTAINERIZATION.md (20 min)
# Testea: Load balancing
```

### Día 3️⃣: Kubernetes
```bash
./supermarket-cli.sh up 3
# Lee: TESTING_GUIDE.md Test 3 (30 min)
# Testea: Auto-scaling
```

### Día 4️⃣: Producción
```bash
./supermarket-cli.sh up 4
# Lee: INTEGRATION_GUIDE.md (15 min)
# Configura: Certificados SSL
```

---

## 🎯 Accesos Rápidos por Versión

### Desarrollo
```
API:        http://localhost:5000
Swagger:    http://localhost:5000/swagger
DB:         localhost:1433
```

### Microservicios
```
API (LB):   http://localhost:5000
Instancias: api-1, api-2
DB:         localhost:1433
```

### Kubernetes
```bash
# Port-forward primero
kubectl port-forward svc/supermercado-api-service 5000:80 -n supermercado

# Luego
API:        http://localhost:5000
```

### Producción
```
API:        https://tu-dominio.com
SSL:        Configurado
DB:         Interno
```

---

## 🔧 Comandos Más Comunes

```bash
# Levantar
./supermarket-cli.sh up 1

# Ver logs
./supermarket-cli.sh logs 1

# Ver estado
./supermarket-cli.sh status 1

# Escalar (solo Microservicios/K8s)
./supermarket-cli.sh scale 2

# Testear
./supermarket-cli.sh test 1

# Detener
./supermarket-cli.sh down 1

# Limpiar todo
./supermarket-cli.sh clean
```

---

## 📂 Estructura del Proyecto

```
supermercado-api/
│
├─ 🎯 CLI MAESTRO (Empieza por aquí)
│  ├─ supermarket-cli.sh        ← Linux/Mac
│  └─ supermarket-cli.ps1       ← Windows
│
├─ 🐳 DOCKER (4 versiones)
│  ├─ Dockerfile
│  ├─ Dockerfile.prod
│  ├─ docker-compose.yml        ← Desarrollo
│  ├─ docker-compose.microservices.yml
│  ├─ docker-compose.prod.yml
│  ├─ docker-compose.master.yml
│  ├─ nginx.conf
│  ├─ nginx.dev.conf
│  └─ nginx.microservices.conf
│
├─ ☸️  KUBERNETES
│  ├─ kubernetes-deployment.yml
│  ├─ kubernetes-sqlserver.yml
│  └─ deploy-k8s.sh
│
├─ 📖 DOCUMENTACIÓN (Lee en este orden)
│  ├─ 👉 SETUP.md               ← EMPIEZA AQUÍ
│  ├─ 👉 INTEGRATION_GUIDE.md    ← Luego aquí
│  ├─ CONTAINERIZATION.md
│  ├─ TESTING_GUIDE.md
│  ├─ ARCHITECTURE.md
│  ├─ FILES_INDEX.md
│  ├─ README_DOCKER.md
│  └─ CONTAINERIZATION_SUMMARY.md
│
├─ 🔧 SCRIPTS DE AUTOMATIZACIÓN
│  ├─ build-images.sh
│  ├─ scale.sh
│  └─ docker-commands.sh
│
└─ ⚙️ CONFIGURACIÓN
   ├─ .env.example             ← Copiar a .env.local
   ├─ .env.local               ← Tu configuración
   ├─ .dockerignore
   └─ .gitignore
```

---

## ✨ Características

### 🟢 Desarrollo
- ✅ 1 API + DB + Load Balancer
- ✅ Hot reload
- ✅ Debugging fácil
- ✅ Perfecto para desarrollo local

### 🟡 Microservicios
- ✅ 2+ instancias de API
- ✅ Load balancing (Nginx)
- ✅ Health checks automáticos
- ✅ Tolerancia a fallos
- ✅ Perfecto para testing

### 🔴 Kubernetes
- ✅ Auto-scaling (2-10 replicas)
- ✅ Orquestación automática
- ✅ Rolling updates sin downtime
- ✅ Auto-healing
- ✅ Producción lista

### 🟠 Producción
- ✅ SSL/TLS
- ✅ Nginx proxy reverso
- ✅ Variables seguras
- ✅ Ready para cloud

---

## 🔐 Seguridad

| Versión | HTTP | HTTPS | Auth | Secrets |
|---------|------|-------|------|---------|
| Desarrollo | ✅ | ❌ | ❌ | ENV |
| Microservicios | ✅ | ❌ | ❌ | ENV |
| Kubernetes | ✅ | 🔧 | ✅ | K8s |
| Producción | ❌ | ✅ | ✅ | ENV |

---

## 🚨 Troubleshooting Rápido

### Puerto en uso
```bash
# Windows
netstat -ano | findstr :5000

# Linux/Mac
lsof -i :5000
```

### No conecta a DB
```bash
./supermarket-cli.sh logs 1
# Ver último error
```

### Kubernetes no funciona
```bash
kubectl cluster-info
# Si falla, iniciar Docker Desktop
```

### Reiniciar desde cero
```bash
./supermarket-cli.sh clean
./supermarket-cli.sh up 1
```

---

## 📊 Comparativa: ¿Cuál Elegir?

```
¿Necesitas...?

□ Solo desarrollo local
  → DESARROLLO (5 min)

□ Testear load balancing
  → MICROSERVICIOS (5 min)

□ Producción con auto-scaling
  → KUBERNETES (20 min)

□ Producción con SSL
  → PRODUCCIÓN (30 min)
```

---

## 🎓 Próximos Pasos

1. **Ahora (5 min)**
   ```bash
   ./supermarket-cli.sh up 1
   curl http://localhost:5000/swagger
   ```

2. **Luego (15 min)**
   - Leer [SETUP.md](SETUP.md)
   - Entender la versión elegida

3. **Después (1 hora)**
   - Leer [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md)
   - Probar siguiente versión

4. **Esta semana**
   - Implementar en tu entorno
   - Customizar según necesidades

---

## 💡 Tips Pro

### 💡 Usar CLI Maestro
No memorices comandos, usa el CLI:
```bash
./supermarket-cli.sh help
```

### 💡 Guardar logs siempre
Antes de reportar problemas:
```bash
./supermarket-cli.sh logs 1 > debug.log
```

### 💡 Testear cada versión
Desarrollo → Microservicios → Kubernetes → Producción

### 💡 Ambiente por rama
- main → Producción
- staging → Kubernetes
- develop → Microservicios
- feature/* → Desarrollo

---

## 📞 Recursos

| Tipo | Recurso |
|------|---------|
| **Inicio** | [SETUP.md](SETUP.md) |
| **Integración** | [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) |
| **Arquitectura** | [ARCHITECTURE.md](ARCHITECTURE.md) |
| **Testing** | [TESTING_GUIDE.md](TESTING_GUIDE.md) |
| **Referencia** | [FILES_INDEX.md](FILES_INDEX.md) |
| **CLI** | `./supermarket-cli.sh help` |

---

## ✅ Estado del Proyecto

| Aspecto | Estado |
|---------|--------|
| Desarrollo | ✅ Completo |
| Microservicios | ✅ Completo |
| Kubernetes | ✅ Completo |
| Producción | ✅ Completo |
| Documentación | ✅ Completa |
| Automatización | ✅ Completa |
| Testing | ✅ Completo |
| **LISTO PARA** | **✅ PRODUCCIÓN** |

---

## 📄 Versión & Licencia

- **Versión:** 2.0
- **Última actualización:** Mayo 2026
- **Estado:** ✅ Listo para Producción

---

## 🚀 ¡VAMOS!

```bash
# COPIAR Y PEGAR AHORA:

cp .env.example .env.local
./supermarket-cli.sh up 1
curl http://localhost:5000/swagger

# ¡LISTO! Tu API está corriendo 🎉
```

---

**¿Preguntas?** Ver [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) o [FILES_INDEX.md](FILES_INDEX.md)

**¿Problemas?** Ver [TESTING_GUIDE.md](TESTING_GUIDE.md) > Troubleshooting


