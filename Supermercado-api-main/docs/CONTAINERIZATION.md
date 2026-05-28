# 🚀 Containerización Escalable de APIs - Supermercado

## 📊 Configuraciones Disponibles

### 1️⃣ **Desarrollo Simple** (docker-compose.yml)
- 1 instancia de API
- 1 SQL Server
- Nginx integrado como reverse proxy
- Mejor para desarrollo local

```bash
docker-compose up -d
# Acceso: http://localhost:5000
```

### 2️⃣ **Microservicios** (docker-compose.microservices.yml)
- 2 instancias de API
- 1 SQL Server compartida
- Nginx con load balancing
- Mejor para testing y staging

```bash
docker-compose -f docker-compose.microservices.yml up -d
# Acceso: http://localhost:5000
# Nginx distribuye carga entre api-1 y api-2
```

### 3️⃣ **Kubernetes** (kubernetes-*.yml)
- Escalado horizontal automático (2-10 replicas)
- StatefulSet para SQL Server
- Auto-scaling por CPU y memoria
- Mejor para producción en cloud

```bash
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-deployment\.yml
```

---

## 🔄 Comparativa

| Feature | Docker Dev | Microservicios | Kubernetes |
|---------|-----------|-----------------|-----------|
| Instancias API | 1 | 2-N | 2-10 (auto) |
| Load Balancer | Nginx | Nginx | K8s |
| Auto-scaling | ❌ | ❌ | ✅ |
| Persistencia | ✅ | ✅ | ✅ |
| Complejidad | 🟢 Baja | 🟡 Media | 🔴 Alta |
| Producción | ❌ | ⚠️ Parcial | ✅ |

---

## 🎯 Usar Microservicios (Docker Compose)

### Inicio

```bash
# Configurar variables
cp .env.example .env.local

# Levantar con 2 instancias de API
docker-compose -f docker-compose.microservices.yml up -d

# Ver logs
docker-compose -f docker-compose.microservices.yml logs -f

# Estado
docker-compose -f docker-compose.microservices.yml ps
```

### Verificar Load Balancing

```bash
# Hacer múltiples requests para ver distribución
for i in {1..10}; do
  curl -s http://localhost:5000/api/health | jq .
  echo "---"
done
```

### Escalar Dinámicamente

Edita `docker-compose.microservices.yml` y agrega más instancias:

```yaml
api-3:
  build:
    context: .
    dockerfile:\ configs/docker/Dockerfile
  # ... misma configuración que api-1 y api-2
```

Luego:
```bash
docker-compose -f docker-compose.microservices.yml up -d
```

---

## 🚀 Usar Kubernetes

### Requisitos

```bash
# kubectl instalado
kubectl version --client

# Cluster disponible (Docker Desktop, Minikube, EKS, AKS, etc.)
```

### Deploy

```bash
# Crear namespace
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml

# Deploy API
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-deployment\.yml

# Ver estado
kubectl get all -n supermercado

# Ver replicas
kubectl get deployments -n supermercado
```

### Verificar

```bash
# Acceder a la API
kubectl port-forward svc/supermercado-api-service 5000:80 -n supermercado

# Luego en otra terminal
curl http://localhost:5000/swagger
```

### Escalar Manualmente

```bash
# Escalar a 5 replicas
kubectl scale deployment supermercado-api --replicas=5 -n supermercado

# Ver pods
kubectl get pods -n supermercado
```

### Ver Auto-scaling

```bash
# Monitorear HPA
kubectl get hpa -n supermercado -w

# Ver métricas
kubectl top pods -n supermercado
kubectl top nodes
```

### Logs

```bash
# De un pod específico
kubectl logs <pod-name> -n supermercado

# Todos los logs
kubectl logs -f deployment/supermercado-api -n supermercado

# Logs de múltiples pods
kubectl logs -f deployment/supermercado-api --all-containers -n supermercado
```

### Desplegar Nueva Versión

```bash
# Reconstruir imagen
docker build -t supermercado-api:v2 .

# Push a registry (Docker Hub, ECR, etc.)
docker push tu-registry/supermercado-api:v2

# Actualizar deployment
kubectl set image deployment/supermercado-api \
  api=tu-registry/supermercado-api:v2 \
  -n supermercado

# Monitorear rollout
kubectl rollout status deployment/supermercado-api -n supermercado
```

---

## 🔧 Configuración de Nginx Load Balancer

### Estrategia Actual: `least_conn`

Envía conexiones al servidor con menos conexiones activas.

**Otras opciones:**

```nginx
# Round-robin (por defecto)
upstream supermercado_api {
    server api-1:80;
    server api-2:80;
}

# IP Hash (sesiones pegajosas)
upstream supermercado_api {
    ip_hash;
    server api-1:80;
    server api-2:80;
}

# Weighted (proporcional)
upstream supermercado_api {
    server api-1:80 weight=3;
    server api-2:80 weight=1;  # recibe 1/4 del tráfico
}

# Random
upstream supermercado_api {
    random;
    server api-1:80;
    server api-2:80;
}
```

### Health Check

El servidor se marca como "down" después de 3 fallos consecutivos y se reintenta después de 30 segundos:

```nginx
server api-1:80 max_fails=3 fail_timeout=30s;
server api-2:80 max_fails=3 fail_timeout=30s;
```

### Ver Estadísticas

```bash
# Acceso al status de Nginx (desarrollo)
curl http://localhost:5000/nginx-status

# En Kubernetes
kubectl port-forward svc/supermercado-nginx 8080:80 -n supermercado
curl http://localhost:8080/nginx-status
```

---

## 📈 Monitoreo

### Docker Compose

```bash
# Estadísticas en tiempo real
docker stats

# Logs centralizados
docker-compose logs -f

# Metrics de Nginx
curl http://localhost:5000/nginx-status

# Health check
curl http://localhost:5000/health
```

### Kubernetes

```bash
# Recursos utilizados
kubectl top nodes
kubectl top pods -n supermercado

# Eventos
kubectl get events -n supermercado

# Describe deployment
kubectl describe deployment supermercado-api -n supermercado
```

---

## 🔍 Troubleshooting

### API no responde

**Docker:**
```bash
docker-compose -f docker-compose.microservices.yml logs api-1
docker-compose -f docker-compose.microservices.yml logs api-2
```

**Kubernetes:**
```bash
kubectl logs deployment/supermercado-api -n supermercado
kubectl describe pod <pod-name> -n supermercado
```

### Load Balancer no distribuye

```bash
# Verificar que ambas instancias estén sanas
docker-compose -f docker-compose.microservices.yml ps

# Hacer requests y ver en logs
for i in {1..5}; do curl http://localhost:5000/api/test; done

# Ver logs de Nginx
docker-compose -f docker-compose.microservices.yml logs nginx
```

### Base de datos no conecta

```bash
# Verificar SQL Server está corriendo
docker-compose -f docker-compose.microservices.yml logs sqlserver

# Conectar directamente
docker exec -it supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P SuperMercado123!
```

---

## 💡 Mejores Prácticas

✅ **Usa microservicios (Docker Compose) para:**
- Desarrollo colaborativo
- Testing de load balancing
- Simulación de escala
- Ambientes de staging

✅ **Usa Kubernetes para:**
- Producción con alta disponibilidad
- Auto-scaling automático
- Orquestación compleja
- Multi-cloud/multi-región

✅ **Seguridad:**
- Usa secretos para credenciales
- No expongas puertos innecesarios
- Usa HTTPS en producción
- Health checks regulares
- Limita recursos por pod/contenedor

✅ **Performance:**
- Mantén conexiones vivas (keepalive)
- Usa compresión gzip
- Cachea respuestas estáticas
- Limita tamaño de uploads
- Configura timeouts adecuados

---

## 📊 Comandos Rápidos

### Docker Compose Microservicios

```bash
# Levantar
docker-compose -f docker-compose.microservices.yml up -d

# Ver logs
docker-compose -f docker-compose.microservices.yml logs -f

# Detener
docker-compose -f docker-compose.microservices.yml down

# Escalar
docker-compose -f docker-compose.microservices.yml up -d --scale api=3

# Conectar a contenedor
docker-compose -f docker-compose.microservices.yml exec api-1 /bin/bash
```

### Kubernetes

```bash
# Deploy
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-sqlserver\.yml
kubectl\ apply\ -f\ \.\./configs/k8s/kubernetes-deployment\.yml

# Ver
kubectl get all -n supermercado

# Escalar
kubectl scale deployment supermercado-api --replicas=5 -n supermercado

# Logs
kubectl logs -f deployment/supermercado-api -n supermercado

# Actualizar
kubectl set image deployment/supermercado-api api=tu-imagen:v2 -n supermercado

# Limpiar
kubectl delete namespace supermercado
```

---

## 🎯 Próximos Pasos

- [ ] Implementar Circuit Breaker
- [ ] Configurar Distributed Tracing (Jaeger)
- [ ] Agregar Prometheus para métricas
- [ ] Implementar Service Mesh (Istio)
- [ ] Configurar Auto-scaling avanzado
- [ ] Implementar Blue-Green Deployment
- [ ] Configurar Canary Releases

---

**Última actualización**: Mayo 2026  
**Versión**: 2.0 - Containerización Escalable


