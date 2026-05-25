# 🚀 CHEAT SHEET - Comandos Rápidos

## ⚡ TU PRIMER COMANDO (AHORA MISMO)

```bash
./supermarket-cli.sh up 1
# O:
docker-compose up -d
```

**Listo en 2 minutos. Accede a:** http://localhost:5000/swagger

---

## 🎛️ CLI MAESTRO - TODO EN UNO

### Startup
```bash
./supermarket-cli.sh up 1          # Desarrollo
./supermarket-cli.sh up 2          # Microservicios
./supermarket-cli.sh up 3          # Kubernetes
./supermarket-cli.sh up 4          # Producción
```

### Control
```bash
./supermarket-cli.sh down 1        # Detener
./supermarket-cli.sh logs 1        # Ver logs
./supermarket-cli.sh status 1      # Estado
./supermarket-cli.sh scale 2 5     # Escalar a 5
./supermarket-cli.sh test 1        # Ejecutar tests
./supermarket-cli.sh clean         # Limpiar todo
./supermarket-cli.sh help          # Ayuda
```

---

## 🐳 DOCKER COMPOSE DIRECTO

### Desarrollo
```bash
docker-compose up -d               # Levantar
docker-compose logs -f             # Ver logs
docker-compose down                # Detener
docker-compose down -v             # Detener + limpiar
```

### Microservicios
```bash
docker-compose -f docker-compose.microservices.yml up -d
docker-compose -f docker-compose.microservices.yml logs -f
docker-compose -f docker-compose.microservices.yml ps
```

### Producción
```bash
docker-compose -f docker-compose.prod.yml \
  --env-file .env.prod up -d
```

---

## ☸️ KUBERNETES

### Deploy
```bash
./deploy-k8s.sh                    # Deploy automático
# O manual:
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-deployment\.yml
```

### Comandos
```bash
kubectl get pods -n supermercado                    # Ver pods
kubectl logs -f deployment/supermercado-api -n supermercado  # Logs
kubectl scale deployment supermercado-api --replicas=5      # Escalar
kubectl delete namespace supermercado               # Limpiar
```

### Port-Forward
```bash
kubectl port-forward svc/supermercado-api-service 5000:80 -n supermercado
# Luego: http://localhost:5000
```

---

## 🔗 ACCESOS

### Desarrollo/Microservicios
```
API:        http://localhost:5000
Swagger:    http://localhost:5000/swagger
SQL Server: localhost:1433
```

### Kubernetes
```
(Necesita port-forward primero)
API:        http://localhost:5000
Swagger:    http://localhost:5000/swagger
```

### Producción
```
API:        https://tu-dominio.com
Swagger:    https://tu-dominio.com/swagger
```

---

## 🔍 TROUBLESHOOTING

### Ver qué pasó
```bash
./supermarket-cli.sh logs 1
# O:
docker-compose logs api
# O en K8s:
kubectl logs -f deployment/supermercado-api -n supermercado
```

### Puerto en uso
```bash
# Windows:
netstat -ano | findstr :5000

# Linux/Mac:
lsof -i :5000
```

### Reiniciar desde cero
```bash
./supermarket-cli.sh clean
./supermarket-cli.sh up 1
```

### Base de datos
```bash
# Conectar a DB en desarrollo
docker exec -it supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123!
```

---

## 📖 DOCUMENTACIÓN QUICK-START

| Documento | Para Qué | Tiempo |
|-----------|----------|--------|
| [SETUP.md](SETUP.md) | Empezar | 5 min |
| [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) | Elegir versión | 15 min |
| [TESTING_GUIDE.md](TESTING_GUIDE.md) | Validar | 45 min |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Entender | 10 min |

---

## 🚀 INICIO POR VERSIÓN

### 1️⃣ Desarrollo (Recomendado si es tu primer día)
```bash
cp .env.example .env.local
docker-compose up -d
curl http://localhost:5000/swagger
```

### 2️⃣ Microservicios (Testing load balancing)
```bash
cp .env.example .env.local
docker-compose -f docker-compose.microservices.yml up -d
./scale.sh load-test 10
```

### 3️⃣ Kubernetes (Producción con auto-scaling)
```bash
cp .env.example .env.local
./deploy-k8s.sh
kubectl get pods -n supermercado
```

### 4️⃣ Producción (Docker con SSL)
```bash
cp .env.example .env.prod
# Editar .env.prod
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

---

## 🎯 ¿CUÁL VERSIÓN?

```
¿Es tu primer día?
├─ SÍ → Versión 1 (Desarrollo)
└─ NO → ¿Necesitas HTTPS?
        ├─ SÍ → Versión 4 (Producción) u Versión 3 (K8s)
        └─ NO → ¿Tienes Kubernetes?
                ├─ SÍ → Versión 3 (K8s)
                └─ NO → Versión 2 (Microservicios)
```

---

## 📊 COMPARATIVA RÁPIDA

| Feature | Dev | Micro | K8s | Prod |
|---------|-----|-------|-----|------|
| Setup | 2min | 5min | 20min | 30min |
| Instancias | 1 | 2+ | 2-10 | 1-N |
| Auto-scale | ❌ | ❌ | ✅ | ❌ |
| Load Bal | Nginx | Nginx | K8s | Nginx |
| SSL/TLS | ❌ | ❌ | 🔧 | ✅ |

---

## 🔧 ARCHIVOS CLAVE

```
supermarket-cli.sh          → CLI maestro (Bash)
supermarket-cli.ps1         → CLI maestro (PowerShell)
docker-compose.yml          → Desarrollo
docker-compose.microservices.yml → Microservicios
kubernetes-deployment.yml   → K8s
.env.example                → Variables plantilla
.env.local                  → Tu configuración
```

---

## 💡 TIPS

### Tip 1: Usa CLI Maestro
No memorices comandos largos:
```bash
./supermarket-cli.sh help
```

### Tip 2: Siempre ve los logs
Antes de reportar problemas:
```bash
./supermarket-cli.sh logs 1
```

### Tip 3: Testea todas las versiones
Dev → Micro → K8s → Prod

### Tip 4: Guardá tu config
Copia `.env.local` a un lugar seguro

---

## ✅ CHECKLIST INICIAL

- [ ] Copié `.env.example` a `.env.local`
- [ ] Ejecuté `./supermarket-cli.sh up 1`
- [ ] Accedí a http://localhost:5000/swagger
- [ ] Leí [SETUP.md](SETUP.md)
- [ ] Leí [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md)

---

## 🎓 ROADMAP

**Día 1:** `./supermarket-cli.sh up 1` → Lee [SETUP.md](SETUP.md)

**Día 2:** `./supermarket-cli.sh up 2` → Lee [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md)

**Día 3:** `./supermarket-cli.sh up 3` → Lee [TESTING_GUIDE.md](TESTING_GUIDE.md)

**Día 4:** `./supermarket-cli.sh up 4` → Deploy

---

**🚀 ¡EMPEZA AHORA!**

```bash
./supermarket-cli.sh up 1
```

**Documentos:** [SETUP.md](SETUP.md) | [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md) | [FILES_INDEX.md](FILES_INDEX.md)


