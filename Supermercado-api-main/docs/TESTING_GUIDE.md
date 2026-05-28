# 🧪 Guía de Testing - APIs Containerizadas

## Test 1: Desarrollo Simple (5 min)

### Setup
```bash
docker-compose up -d
```

### Verificar
```bash
# Estado
docker-compose ps

# Acceso
curl http://localhost:5000/swagger

# Logs
docker-compose logs -f api
```

### Resultado Esperado
```
✅ 1 API corriendo
✅ SQL Server accesible
✅ Swagger disponible
✅ Health check OK
```

### Cleanup
```bash
docker-compose down
```

---

## Test 2: Microservicios (10 min)

### Setup
```bash
docker-compose -f docker-compose.microservices.yml up -d
```

### Verificar Instancias
```bash
docker-compose -f docker-compose.microservices.yml ps

# Resultado esperado:
# supermercado-sqlserver    ✔ Running
# supermercado-api-1        ✔ Running
# supermercado-api-2        ✔ Running
# supermercado-nginx        ✔ Running
```

### Verificar Load Balancing
```bash
# Test 1: Acceso a Swagger
curl http://localhost:5000/swagger

# Test 2: Múltiples requests (ver distribución)
for i in {1..10}; do
  echo "Request $i:"
  curl -s http://localhost:5000/api/health | jq .
  echo "---"
done

# Test 3: Ver logs de distribución
docker-compose -f docker-compose.microservices.yml logs nginx | grep upstream
```

### Verificar Health Check
```bash
# Health individual de api-1
curl http://$(docker inspect -f '{{.NetworkSettings.IPAddress}}' supermercado-api-1):80/health

# Health individual de api-2
curl http://$(docker inspect -f '{{.NetworkSettings.IPAddress}}' supermercado-api-2):80/health

# A través de Nginx (load balancer)
curl http://localhost:5000/health
```

### Test de Tolerancia a Fallos
```bash
# Detener una instancia
docker-compose -f docker-compose.microservices.yml stop api-1

# Hacer requests (deben ir a api-2)
for i in {1..5}; do
  curl http://localhost:5000/health
done

# Reanudar
docker-compose -f docker-compose.microservices.yml start api-1

# Nginx detecta que está sana nuevamente
docker-compose -f docker-compose.microservices.yml logs nginx
```

### Resultado Esperado
```
✅ 2 instancias de API corriendo
✅ Nginx distribuye carga
✅ Health checks automáticos
✅ Tolerancia a fallos
✅ Logs centralizados
```

### Cleanup
```bash
docker-compose -f docker-compose.microservices.yml down
```

---

## Test 3: Kubernetes (15 min)

### Requisitos
```bash
# Verificar kubectl
kubectl version --client

# Verificar cluster
kubectl cluster-info
```

### Setup
```bash
# Deploy SQL Server
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml

# Deploy API
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-deployment\.yml
```

### Verificar Deployment
```bash
# Ver namespace
kubectl get ns | grep supermercado

# Ver pods
kubectl get pods -n supermercado
# Resultado esperado: 3 pods de API (replicas)

# Ver servicios
kubectl get svc -n supermercado

# Ver deployments
kubectl get deployments -n supermercado

# Ver HPA (Auto-scaling)
kubectl get hpa -n supermercado
```

### Acceso a la API
```bash
# Port-forward
kubectl port-forward svc/supermercado-api-service 5000:80 -n supermercado

# En otra terminal
curl http://localhost:5000/swagger
```

### Test de Auto-scaling
```bash
# Monitorear HPA
kubectl get hpa -n supermercado -w

# Generar carga (en otra terminal)
for i in {1..1000}; do
  curl http://localhost:5000/health &
done

# Observar: Los pods deberían incrementarse hasta 10 máximo
kubectl get pods -n supermercado -w

# Esperar a que la carga disminuya
# Los pods deberían decrementarse de vuelta a 2-3

# Detener el port-forward con Ctrl+C
```

### Test de Rolling Update
```bash
# Ver replicas actuales
kubectl get deployments supermercado-api -n supermercado

# Actualizar versión (simular nueva build)
kubectl set image deployment/supermercado-api \
  api=supermercado-api:v2 \
  -n supermercado --record

# Monitorear rollout
kubectl rollout status deployment/supermercado-api -n supermercado -w

# Resultado: Nuevos pods se crean, viejos se eliminan gradualmente
kubectl get pods -n supermercado -w
```

### Test de Tolerancia a Fallos
```bash
# Eliminar un pod
kubectl delete pod <pod-name> -n supermercado

# Resultado: Se crea automáticamente un nuevo pod
kubectl get pods -n supermercado -w
```

### Ver Logs
```bash
# Logs de un pod
kubectl logs <pod-name> -n supermercado

# Logs de todos los pods
kubectl logs -f deployment/supermercado-api -n supermercado
```

### Ver Métricas
```bash
# CPU y Memoria
kubectl top pods -n supermercado
kubectl top nodes

# Describir deployment
kubectl describe deployment supermercado-api -n supermercado

# Ver eventos
kubectl get events -n supermercado
```

### Resultado Esperado
```
✅ 3 replicas de API corriendo
✅ Auto-scaling funciona (CPU/Memoria)
✅ Rolling updates sin downtime
✅ Auto-healing de pods
✅ Logs centralizados
✅ Métricas disponibles
```

### Cleanup
```bash
kubectl delete namespace supermercado
```

---

## Test 4: Comparativa de Performance

### Preparar
```bash
# Docker Compose Dev
docker-compose up -d
COMPOSE_PID=$!

# Esperar a estar listo
sleep 5

# Ejecutar benchmark
echo "=== Docker Compose (1 instancia) ===" 
ab -n 1000 -c 10 http://localhost:5000/health

docker-compose down
```

```bash
# Docker Compose Microservicios
docker-compose -f docker-compose.microservices.yml up -d

# Esperar a estar listo
sleep 10

# Ejecutar benchmark
echo "=== Microservicios (2 instancias) ===" 
ab -n 1000 -c 10 http://localhost:5000/health

docker-compose -f docker-compose.microservices.yml down
```

### Resultado Esperado
```
Microservicios debería mostrar:
- Mayor throughput
- Menor tiempo de respuesta promedio
- Mejor manejo de carga simultánea
```

---

## Test 5: Database Persistence

### Test Docker Compose
```bash
# Levantar
docker-compose up -d

# Crear datos de prueba
docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "CREATE TABLE test (id INT, name VARCHAR(50)); INSERT INTO test VALUES (1, 'test')"

# Detener sin eliminar volúmenes
docker-compose down

# Verificar que volumen persiste
docker volume ls | grep supermercado

# Levantar de nuevo
docker-compose up -d

# Verificar que datos están
docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123! \
  -Q "SELECT * FROM test"

# Resultado: Fila con id=1, name='test' debe aparecer
```

### Test Kubernetes
```bash
# Deploy
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml

# Crear datos (via port-forward a DB)
kubectl port-forward svc/sqlserver 1433:1433 -n supermercado &

sqlcmd -S localhost -U sa -P YourPassword \
  -Q "CREATE TABLE test (id INT, name VARCHAR(50)); INSERT INTO test VALUES (1, 'test')"

# Recrear pod (simular fallo)
kubectl delete pod -l app=sqlserver -n supermercado

# Monitorear recreación
kubectl get pods -n supermercado -w

# Esperar a que esté listo
sleep 30

# Verificar datos
sqlcmd -S localhost -U sa -P YourPassword \
  -Q "SELECT * FROM test"

# Resultado: Fila debe persistir
```

---

## Test 6: Escalado Manual

### Docker Compose
```bash
# Editar docker-compose.microservices.yml
# Cambiar servicios: copiar api-2 a api-3, api-4, etc.

# Levantar
docker-compose -f docker-compose.microservices.yml up -d

# Ver instancias
docker-compose -f docker-compose.microservices.yml ps

# Load test
for i in {1..100}; do
  curl http://localhost:5000/health &
done
```

### Kubernetes
```bash
# Escalar a 5 replicas
kubectl scale deployment supermercado-api --replicas=5 -n supermercado

# Ver creación de nuevos pods
kubectl get pods -n supermercado -w

# Escalar hacia abajo
kubectl scale deployment supermercado-api --replicas=2 -n supermercado

# Ver eliminación de pods
kubectl get pods -n supermercado -w
```

---

## Checklist de Validación

- [ ] **Desarrollo Simple**: 1 API + DB + Swagger
- [ ] **Microservicios**: 2 APIs + Load Balancer + Nginx
- [ ] **Kubernetes**: 3 replicas + Auto-scaling + Rolling updates
- [ ] **Load Balancing**: Distribución entre instancias
- [ ] **Health Checks**: Automáticos cada 30s
- [ ] **Tolerancia a Fallos**: Reemplazo automático
- [ ] **Persistencia**: Datos se mantienen
- [ ] **Escalado**: Manual y automático
- [ ] **Logs**: Centralizados y accesibles
- [ ] **Performance**: Mejorado con múltiples instancias

---

## Troubleshooting

### Docker no responde
```bash
docker ps
# Si falla, reiniciar Docker Desktop
```

### Puerto en uso
```bash
# Windows
netstat -ano | findstr :5000

# Linux/Mac
lsof -i :5000
```

### Kubernetes no conecta
```bash
kubectl cluster-info
# Si falla, iniciar Docker Desktop/Minikube
```

### Pods en error
```bash
kubectl describe pod <pod-name> -n supermercado
kubectl logs <pod-name> -n supermercado
```

---

## 📊 Resultados Esperados

| Test | Esperado | Resultado |
|------|----------|-----------|
| Dev simple | ✅ 1 API | ✓ |
| Microservicios | ✅ 2 APIs + LB | ✓ |
| Kubernetes | ✅ 3 replicas | ✓ |
| Auto-scaling | ✅ Hasta 10 | ✓ |
| Persistencia | ✅ Datos OK | ✓ |
| Health check | ✅ Automático | ✓ |

---

**Tiempo total estimado:** 45 minutos
**Nivel de dificultad:** 🟡 Intermedio


