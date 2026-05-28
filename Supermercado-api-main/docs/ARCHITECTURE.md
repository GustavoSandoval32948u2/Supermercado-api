# 🏗️ Arquitectura Visual - Supermercado API

## Opción 1: Desarrollo Simple

```
┌─────────────────────────────────────────────────────────┐
│                    HOST MACHINE                         │
└─────────────────────────────────────────────────────────┘
              ↓
     ┌──────────────────┐
     │ Docker Network   │
     │ supermercado-net │
     └──────────────────┘
         ↓              ↓
    ┌────────┐    ┌──────────────┐
    │  API   │←---→│  SQL Server  │
    │  :80   │    │   :1433      │
    └────────┘    └──────────────┘
         ↓
    ┌────────┐
    │ Nginx  │
    │ :5000  │
    └────────┘
         ↓
    localhost:5000/swagger
```

**Características:**
- 1 instancia de API
- 1 SQL Server
- Nginx como reverse proxy
- Perfecto para desarrollo local

---

## Opción 2: Microservicios

```
┌─────────────────────────────────────────────────────────┐
│                    HOST MACHINE                         │
└─────────────────────────────────────────────────────────┘
              ↓
     ┌──────────────────┐
     │ Docker Network   │
     │ supermercado-net │
     └──────────────────┘
         ↓
    ┌────────────────────────────────────┐
    │        NGINX (Load Balancer)       │
    │  Least Conn, Health Checks         │
    │          :5000                     │
    └────────────────────────────────────┘
     ↙                    ↘
┌─────────┐        ┌─────────┐
│ API-1   │        │ API-2   │
│ :80     │        │ :80     │
└─────────┘        └─────────┘
    ↓                  ↓
    └──────────┬───────┘
               ↓
        ┌──────────────┐
        │  SQL Server  │
        │   :1433      │
        └──────────────┘

Distribución de carga:
Request 1 → API-1
Request 2 → API-2
Request 3 → API-1 (menos conexiones)
...
```

**Características:**
- 2-N instancias de API
- 1 SQL Server compartida
- Nginx con load balancing (least_conn)
- Health checks automáticos
- Tolerancia a fallos

---

## Opción 3: Kubernetes

```
┌─────────────────────────────────────────────────────────┐
│              Kubernetes Cluster                         │
│              (Docker Desktop, EKS, AKS, etc)            │
└─────────────────────────────────────────────────────────┘
                      ↓
            ┌─────────────────────┐
            │  supermercado NS    │
            └─────────────────────┘
                      ↓
    ┌─────────────────────────────────┐
    │  Ingress / Load Balancer        │
    │  (K8s Service - LoadBalancer)   │
    └─────────────────────────────────┘
                      ↓
    ┌─────────────────────────────────┐
    │  Deployment (Auto-scaling)      │
    │  HPA: 2-10 replicas             │
    │  CPU 70%, Memory 80%            │
    └─────────────────────────────────┘
     ↙        ↓         ↓        ↘
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│Pod 1   │ │Pod 2   │ │Pod 3   │ │Pod ... │
│API :80 │ │API :80 │ │API :80 │ │API :80 │
└────────┘ └────────┘ └────────┘ └────────┘
    ↓         ↓         ↓         ↓
    └─────────────┬────────────────┘
                  ↓
        ┌──────────────────┐
        │  StatefulSet     │
        │  SQL Server      │
        │  :1433           │
        │  (Persistent PVC)│
        └──────────────────┘

Auto-scaling en acción:
- CPU > 70% → Agregar pod
- CPU < 50% → Remover pod
- Máximo 10 replicas
```

**Características:**
- 2-10 replicas (auto-scaling)
- 1 SQL Server como StatefulSet
- Orquestación automática
- RollingUpdate (sin downtime)
- Auto-healing
- Monitoring integrado

---

## Flujo de Datos: Opción 2 (Microservicios)

```
Cliente HTTP Request
         ↓
    ┌────────────┐
    │   Nginx    │  ← Recibe request en :5000
    │LoadBalancer│  ← Aplica Health Check
    │Health↻    │  ← Registra en logs
    └────────────┘
         ↓
    ¿Cuál API?
    ├─ API-1 (5 conexiones activas)
    ├─ API-2 (3 conexiones activas) ← Usa este (least_conn)
    └─ API-3 (7 conexiones activas)
         ↓
    ┌────────────┐
    │   API-2    │  ← Procesa request
    │ .NET 8.0   │  ← Valida
    │Contro...   │  ← Busca en BD
    └────────────┘
         ↓
    ┌────────────────┐
    │  SQL Server    │  ← Query a BD
    │SELECT * FROM..│
    └────────────────┘
         ↓
    Response generada
         ↓
    ┌────────────┐
    │   Nginx    │  ← Compresión Gzip
    │Reverse     │  ← Agrega headers
    │Proxy       │  ← Caching
    └────────────┘
         ↓
    Cliente recibe respuesta
```

---

## Health Check Flow

```
Cada 30 segundos:
    ↓
┌─────────────────┐
│  Nginx          │
│  Health Check   │
└─────────────────┘
    ↙              ↘
GET /health      GET /health
    ↓                ↓
┌────────┐        ┌────────┐
│ API-1  │        │ API-2  │
│ ✅ 200 │        │ ✅ 200 │
└────────┘        └────────┘

Resultado:
✅ Ambos sanos → Mantener
⚠️ API-1 timeout → Marcar como "down"
❌ API-2 timeout 3x → Remover por 30s

Reintentar en 30 segundos
```

---

## Load Balancing Strategies

### least_conn (Actual)
```
API-1: 5 conexiones → No envía aquí
API-2: 2 conexiones → Envía aquí ✓
API-3: 4 conexiones → No envía aquí

Próximo request → API-2 (o API-3 si se completa una conn en API-1)
```

### round_robin (Alternativa)
```
API-1 → API-2 → API-3 → API-1 → ...
1era   2da    3era    4ta
```

### ip_hash (Sesiones pegajosas)
```
IP 192.168.1.1 → Siempre a API-1
IP 192.168.1.2 → Siempre a API-2
IP 192.168.1.3 → Siempre a API-3
```

### weighted (Carga proporcional)
```
API-1: weight=3 → 75% del tráfico
API-2: weight=1 → 25% del tráfico
```

---

## Escalado: Kubernetes

```
MOMENTO 1: Carga normal
┌────────┐ ┌────────┐ ┌────────┐
│Pod-1   │ │Pod-2   │ │Pod-3   │
│CPU: 20%│ │CPU: 30%│ │CPU: 25%│
└────────┘ └────────┘ └────────┘

↓ Aumenta tráfico ↓

MOMENTO 2: Carga alta (CPU > 70%)
HPA detecta: "CPU promedio = 75%"
Crear nuevos pods...

┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│Pod-1   │ │Pod-2   │ │Pod-3   │ │Pod-4   │ │Pod-5   │
│CPU: 65%│ │CPU: 68%│ │CPU: 72%│ │CPU: 64%│ │CPU: 66%│
└────────┘ └────────┘ └────────┘ └────────┘ └────────┘

↓ Disminuye tráfico ↓

MOMENTO 3: Carga normal (CPU < 50%)
HPA detecta: "CPU promedio = 45%"
Eliminar pods...

┌────────┐ ┌────────┐
│Pod-1   │ │Pod-2   │
│CPU: 50%│ │CPU: 48%│
└────────┘ └────────┘
```

---

## Persistencia: Volúmenes

```
┌──────────────────────────┐
│   Docker Host            │
│   /var/lib/docker/       │
│   volumes/               │
└──────────────────────────┘
    ↙        ↓        ↘
    
┌─────────┐ ┌──────┐ ┌────────┐
│ data    │ │ logs │ │ backup │
│  10GB   │ │ 1GB  │ │  5GB   │
└─────────┘ └──────┘ └────────┘
    ↑        ↑        ↑
    └────────┼────────┘
             ↓
    ┌──────────────────┐
    │  SQL Server      │
    │  Container       │
    │  Monta volumes   │
    └──────────────────┘

Persistencia:
docker-compose down  → Datos se mantienen
docker-compose down -v → Datos se eliminan
```

---

## Seguridad: Layering

```
┌─────────────────────────────────────────┐
│         INTERNET                        │
└─────────────────────────────────────────┘
                ↓
┌─────────────────────────────────────────┐
│   Nginx (HTTPS)                         │
│   - SSL/TLS                             │
│   - Rate limiting                       │
│   - Headers de seguridad                │
│   - CORS                                │
└─────────────────────────────────────────┘
                ↓
┌─────────────────────────────────────────┐
│   API Container                         │
│   - Usuario no-root (uid 1000)          │
│   - Sin privilegios                     │
│   - Filesystem read-only                │
│   - Secrets en env vars                 │
└─────────────────────────────────────────┘
                ↓
┌─────────────────────────────────────────┐
│   SQL Server Container                  │
│   - Red privada (no expuesta)           │
│   - Credenciales en secrets             │
│   - Backups encriptados                 │
│   - Volumen encriptado                  │
└─────────────────────────────────────────┘
```

---

## Monitoreo: Puntos de Observación

```
┌──────────────────────────────────────────┐
│         PROMETHEUS                       │
│         (Métricas)                       │
└──────────────────────────────────────────┘
    ↑        ↑        ↑        ↑
    └────┬───┴────┬───┴────┬───┘
         ↓        ↓        ↓
    ┌────────┐ ┌────────┐ ┌──────────┐
    │ Nginx  │ │ APIs   │ │SQL Server│
    │Métricas│ │Métricas│ │Métricas  │
    └────────┘ └────────┘ └──────────┘

Eventos registrados:
- Request rate
- Response time
- Error rate
- CPU/Memory
- Conexiones de BD
- Health checks
- Pod restarts
- Escalado eventos
```

---

## Comparison Matrix

```
                    │ Dev    │ Microserv │ K8s
────────────────────┼────────┼───────────┼─────
Instancias API      │ 1      │ 2-N       │ 2-10
Auto-scaling        │ ❌     │ ❌        │ ✅
Load Balancer       │ Nginx  │ Nginx     │ K8s
Health Checks       │ ✅     │ ✅        │ ✅
Persistencia        │ ✅     │ ✅        │ ✅
Complejidad         │ 🟢     │ 🟡        │ 🔴
Setup (min)         │ 2      │ 5         │ 20
Costo (relativo)    │ $      │ $$        │ $$$
Producción listo    │ ❌     │ ⚠️         │ ✅
────────────────────┴────────┴───────────┴─────
```

---

## Timeline de Deploy

```
Minuto 0:  docker-compose up -d
├─ Pulling images...
├─ Creating network...
├─ Starting sqlserver...
├─ Waiting for DB (20s)
└─ Starting api + nginx...

Minuto 1: ✅ Listo
├─ http://localhost:5000 ✓
├─ Health check pasado ✓
└─ Logs disponibles ✓

Minuto 2-60: Operación Normal
├─ Nginx distribuye carga
├─ Health checks cada 30s
├─ Logs en tiempo real
└─ Métricas registradas

Minuto 61+: Mantenimiento
├─ Backup de BD
├─ Rotación de logs
├─ Limpieza de contenedores
└─ Actualización de imagen
```

---

**Última actualización:** Mayo 2026


