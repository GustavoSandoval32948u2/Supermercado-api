# 📚 GUÍA DE INTEGRACIÓN FINAL - Supermercado API

## 🎯 4 Versiones Integradas

Este proyecto incluye **4 arquitecturas completas** que puedes elegir según tus necesidades:

```
┌─────────────────────────────────────────────────────────────┐
│                  SUPERMERCADO API                           │
│                   Todas las Versiones                       │
└─────────────────────────────────────────────────────────────┘
          │
    ┌─────┼─────┬────────┐
    │     │     │        │
    ▼     ▼     ▼        ▼
  Dev   Micro  K8s    Prod
   1     2      3       4
```

---

## 🎛️ SELECTOR DE VERSIÓN

### **Opción 1: CLI Maestro (RECOMENDADO) 🌟**

Gestiona todas las versiones desde un único comando:

**Linux/Mac:**
```bash
./supermarket-cli.sh help
./supermarket-cli.sh up 1        # Desarrollo
./supermarket-cli.sh up 2        # Microservicios
./supermarket-cli.sh up 3        # Kubernetes
./supermarket-cli.sh up 4        # Producción
```

**Windows:**
```powershell
.\supermarket-cli.ps1 help
.\supermarket-cli.ps1 up 1
.\supermarket-cli.ps1 up 2
.\supermarket-cli.ps1 up 3
.\supermarket-cli.ps1 up 4
```

### **Opción 2: Docker Compose Directo**

```bash
# Desarrollo
docker-compose up -d

# Microservicios
docker-compose -f docker-compose.microservices.yml up -d

# Producción
docker-compose -f docker-compose.prod.yml up -d
```

### **Opción 3: Kubernetes Directo**

```bash
./deploy-k8s.sh

# O manualmente:
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-deployment\.yml
```

---

## 📋 VERSIÓN 1: DESARROLLO

### 📊 Composición
- **1 API** en .NET 8.0
- **1 SQL Server** 2022
- **1 Nginx** (Reverse Proxy)

### ⚡ Inicio Rápido
```bash
./supermarket-cli.sh up 1
# O:
docker-compose up -d
```

### 📍 Acceso
```
API:           http://localhost:5000
Swagger:       http://localhost:5000/swagger
SQL Server:    localhost:1433
```

### 📖 Documentación
- [SETUP.md](SETUP.md) - Guía de inicio
- [README_DOCKER.md](README_DOCKER.md) - Docker básico
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Test 1

### ✅ Ideal Para
- Desarrollo local
- Testing inicial
- Documentación
- Debugging

---

## 📋 VERSIÓN 2: MICROSERVICIOS

### 📊 Composición
- **2 APIs** en .NET 8.0
- **1 SQL Server** 2022
- **1 Nginx** Load Balancer (least_conn)
- Health checks automáticos

### ⚡ Inicio Rápido
```bash
./supermarket-cli.sh up 2
# O:
docker-compose -f docker-compose.microservices.yml up -d
```

### 📍 Acceso
```
API (Load Balanced):  http://localhost:5000
Swagger:              http://localhost:5000/swagger
SQL Server:           localhost:1433
Instancia 1:          supermercado-api-1
Instancia 2:          supermercado-api-2
```

### 🔄 Escalado
```bash
# Escalar a 5 instancias
./supermarket-cli.sh scale 2

# O manualmente editar docker-compose.microservices.yml
```

### 📖 Documentación
- [CONTAINERIZATION.md](CONTAINERIZATION.md) - Arquitectura
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Test 2
- [ARCHITECTURE.md](ARCHITECTURE.md) - Diagramas

### ✅ Ideal Para
- Testing de load balancing
- Simulación de múltiples instancias
- Staging pre-kubernetes
- Validar tolerancia a fallos

---

## 📋 VERSIÓN 3: KUBERNETES

### 📊 Composición
- **3-10 Replicas de API** (auto-scaling)
- **1 SQL Server** como StatefulSet
- **Ingress/LoadBalancer** de K8s
- **HPA** (Horizontal Pod Autoscaler)

### ⚡ Inicio Rápido
```bash
./supermarket-cli.sh up 3
# O:
./deploy-k8s.sh
```

### 📍 Acceso
```bash
# Port-forward a local
kubectl port-forward svc/supermercado-api-service 5000:80 -n supermercado

# Luego:
API:           http://localhost:5000
Swagger:       http://localhost:5000/swagger
```

### 🔄 Escalado Automático
```bash
# Monitorear auto-scaling
kubectl get hpa -n supermercado -w

# Escalar manualmente
kubectl scale deployment supermercado-api --replicas=10 -n supermercado
```

### 📖 Documentación
- [CONTAINERIZATION.md](CONTAINERIZATION.md) - Kubernetes
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Test 3
-\ \[kubernetes-deployment\.yml]\(\.\./configs/k8s/kubernetes-deployment\.yml\)

### ✅ Ideal Para
- **Producción** con alta disponibilidad
- Auto-scaling automático
- Orquestación compleja
- Multi-cloud/multi-región

---

## 📋 VERSIÓN 4: PRODUCCIÓN

### 📊 Composición
- **1 API** (o múltiples con escalado manual)
- **1 SQL Server** 2022
- **1 Nginx** Proxy Reverso (SSL/TLS)
- Variables de entorno seguras

### ⚡ Inicio Rápido
```bash
# Preparar
cp .env.example .env.prod
# Editar .env.prod con valores reales

# Levantar
./supermarket-cli.sh up 4
# O:
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

### 🔐 Seguridad
```bash
# Generar certificados SSL
mkdir certs
certbot certonly --standalone -d tu-dominio.com
cp /etc/letsencrypt/live/tu-dominio.com/fullchain.pem certs/cert.pem
cp /etc/letsencrypt/live/tu-dominio.com/privkey.pem certs/key.pem
```

### 📍 Acceso
```
API (Nginx HTTPS):  https://tu-dominio.com
Swagger:            https://tu-dominio.com/swagger
SQL Server:         (interno, no expuesto)
```

### 📖 Documentación
- [CONTAINERIZATION_SUMMARY.md](CONTAINERIZATION_SUMMARY.md)
- [docker-compose.prod.yml](docker-compose.prod.yml)
-\ \[nginx\.conf]\(\.\./configs/nginx/nginx\.conf\)

### ✅ Ideal Para
- Producción segura
- Dominio propio
- SSL/TLS obligatorio
- Acceso externo controlado

---

## 🔀 COMPARATIVA COMPLETA

| Aspecto | Dev | Micro | K8s | Prod |
|---------|-----|-------|-----|------|
| **Instancias** | 1 | 2+ | 2-10 | 1+ |
| **Auto-scaling** | ❌ | ❌ | ✅ | ❌ |
| **Load Balancer** | Nginx | Nginx | K8s | Nginx |
| **Health Checks** | ✅ | ✅ | ✅ | ✅ |
| **Persistencia** | ✅ | ✅ | ✅ | ✅ |
| **SSL/TLS** | ❌ | ❌ | 🔧 | ✅ |
| **Complejidad** | 🟢 | 🟡 | 🔴 | 🟡 |
| **Setup** | 2 min | 5 min | 20 min | 30 min |
| **Producción** | ❌ | ⚠️ | ✅ | ✅ |
| **Costo** | $ | $$  | $$$  | $$  |

---

## 🚀 FLUJO DE DESARROLLO RECOMENDADO

### Día 1: Local
```bash
# Desarrollo Simple
./supermarket-cli.sh up 1
# Testear API
curl http://localhost:5000/swagger
```

### Día 2: Testing
```bash
# Microservicios
./supermarket-cli.sh up 2
# Testear load balancing
./supermarket-cli.sh test 2
```

### Día 3-7: Staging
```bash
# Kubernetes en cluster de staging
./supermarket-cli.sh up 3
# Validar auto-scaling
kubectl get hpa -n supermercado -w
```

### Semana 2+: Producción
```bash
# Preparar .env.prod
# Configurar certificados SSL
# Deploy
./supermarket-cli.sh up 4
```

---

## 📚 ÁRBOL DE DOCUMENTACIÓN

```
INTEGRATION_GUIDE.md (Este archivo)
├─ Versión 1: Desarrollo
│  ├─ SETUP.md
│  ├─ README_DOCKER.md
│  └─ TESTING_GUIDE.md > Test 1
├─ Versión 2: Microservicios
│  ├─ CONTAINERIZATION.md
│  ├─ TESTING_GUIDE.md > Test 2
│  └─ ARCHITECTURE.md > Opción 2
├─ Versión 3: Kubernetes
│  ├─ CONTAINERIZATION.md > Kubernetes
│  ├─ TESTING_GUIDE.md > Test 3
│  ├─ kubernetes-deployment.yml
│  └─ ARCHITECTURE.md > Opción 3
├─ Versión 4: Producción
│  ├─ CONTAINERIZATION_SUMMARY.md
│  ├─ docker-compose.prod.yml
│  └─ nginx.conf
└─ Referencia
   ├─ FILES_INDEX.md
   ├─ ARCHITECTURE.md
   ├─ docker-compose.master.yml
   └─ supermarket-cli.sh/ps1
```

---

## 🎯 DECISIÓN RÁPIDA

**¿Cuál versión usar?**

```
¿Es tu primer día?
├─ SÍ → Versión 1: Desarrollo
└─ NO → ¿Tienes Kubernetes?
        ├─ SÍ → Versión 3: Kubernetes
        └─ NO → ¿Necesitas HTTPS?
                ├─ SÍ → Versión 4: Producción
                └─ NO → Versión 2: Microservicios
```

---

## 🔧 COMANDOS ESENCIALES

### CLI Maestro (RECOMENDADO)
```bash
# Versión 1
./supermarket-cli.sh up 1
./supermarket-cli.sh down 1
./supermarket-cli.sh logs 1

# Versión 2
./supermarket-cli.sh up 2
./supermarket-cli.sh scale 2
./supermarket-cli.sh test 2

# Versión 3
./supermarket-cli.sh up 3
./supermarket-cli.sh scale 3

# Versión 4
./supermarket-cli.sh up 4
```

### Docker Compose Directo
```bash
# Desarrollo
docker-compose up/down/logs -f

# Microservicios
docker-compose -f docker-compose.microservices.yml up/down/logs -f

# Producción
docker-compose -f docker-compose.prod.yml --env-file .env.prod up/down
```

### Kubernetes Directo
```bash
# Deploy
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-deployment\.yml

# Logs
kubectl logs -f deployment/supermercado-api -n supermercado

# Escalar
kubectl scale deployment supermercado-api --replicas=10 -n supermercado
```

---

## 🆘 TROUBLESHOOTING

### Todos no funcionan
```bash
./supermarket-cli.sh clean
./supermarket-cli.sh up 1  # Reiniciar con Desarrollo
```

### Kubernetes no conecta
```bash
kubectl cluster-info
# Si falla, iniciar Docker Desktop o Minikube
```

### Puertos en uso
```bash
# Ver qué está usando el puerto
# Windows:
netstat -ano | findstr :5000

# Linux/Mac:
lsof -i :5000
```

### Base de datos no conecta
```bash
# Verificar SQL Server
docker logs supermercado-sqlserver

# O en Kubernetes:
kubectl logs -f statefulset/sqlserver -n supermercado
```

---

## ✅ CHECKLIST DE SETUP

- [ ] Leer este documento (5 min)
- [ ] Instalar Docker (si aún no lo hiciste)
- [ ] Clonar/descargar proyecto
- [ ] Copiar `.env.example` a `.env.local`
- [ ] Ejecutar: `./supermarket-cli.sh up 1`
- [ ] Acceder a http://localhost:5000/swagger
- [ ] Ver logs: `./supermarket-cli.sh logs 1`
- [ ] Leer documentación de tu versión elegida

---

## 📞 PRÓXIMOS PASOS

### Después de Desarrollo (1-2 horas)
- Leer [CONTAINERIZATION.md](CONTAINERIZATION.md)
- Probar Versión 2: Microservicios
- Entender Load Balancing

### Después de Microservicios (1-2 horas)
- Instalar Kubernetes (Minikube, Docker Desktop K8s)
- Probar Versión 3: Kubernetes
- Entender Auto-scaling

### Después de Kubernetes (2-4 horas)
- Generar certificados SSL
- Probar Versión 4: Producción
- Implementar CI/CD

### Producción Avanzada (1-2 semanas)
- [ ] Implementar Prometheus + Grafana
- [ ] Centralizar logs (ELK Stack)
- [ ] Configurar alertas
- [ ] Backup automático
- [ ] Disaster recovery
- [ ] Service mesh (Istio)

---

## 🎓 RECURSOS

| Recurso | Descripción |
|---------|------------|
| [supermarket-cli.sh](supermarket-cli.sh) | CLI maestro (Linux/Mac) |
| [supermarket-cli.ps1](supermarket-cli.ps1) | CLI maestro (Windows) |
| [docker-compose.master.yml](docker-compose.master.yml) | Selector de versiones |
| [CONTAINERIZATION.md](CONTAINERIZATION.md) | Guía técnica completa |
| [TESTING_GUIDE.md](TESTING_GUIDE.md) | Guías de testing |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Diagramas y visualización |
| [FILES_INDEX.md](FILES_INDEX.md) | Índice de todos los archivos |

---

## 🌟 TIPS PRO

### 💡 Tip 1: Usar CLI Maestro
Es mucho más fácil que recordar todos los comandos:
```bash
./supermarket-cli.sh help  # Ver todas las opciones
```

### 💡 Tip 2: Guardá tu versión elegida
Una vez que decidas, usa siempre la misma versión en tu .env:
```env
DEPLOYMENT_VERSION=2  # 1=Dev, 2=Micro, 3=K8s, 4=Prod
```

### 💡 Tip 3: Testea antes de producción
Siempre prueba en Desarrollo → Microservicios → Kubernetes antes de Producción

### 💡 Tip 4: Monitoreá los logs
Siempre que levantes algo nuevo:
```bash
./supermarket-cli.sh logs <versión>
```

---

**Estado:** ✅ Completo  
**Última actualización:** Mayo 2026  
**Versión:** 2.0 - Totalmente Integrado


