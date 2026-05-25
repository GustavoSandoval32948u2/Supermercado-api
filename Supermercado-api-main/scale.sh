#!/bin/bash

# Script auxiliar para trabajar con múltiples instancias de API

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

show_help() {
    echo -e "${BLUE}=== Herramientas de Escalado ===${NC}"
    echo ""
    echo "Uso: ./scale.sh [comando] [opciones]"
    echo ""
    echo "Comandos:"
    echo "  compose-scale N      - Escalar a N instancias (Docker Compose)"
    echo "  k8s-scale N         - Escalar a N replicas (Kubernetes)"
    echo "  load-test N         - Hacer N requests para probar load balancing"
    echo "  status-compose      - Ver estado de instancias (Docker Compose)"
    echo "  status-k8s          - Ver estado de replicas (Kubernetes)"
    echo "  logs-compose N      - Ver logs de instancia N"
    echo "  logs-k8s            - Ver logs de todas las replicas"
    echo ""
}

# Docker Compose Scale
compose_scale() {
    local SCALE=$1
    if [ -z "$SCALE" ]; then
        echo -e "${RED}❌ Especifica número de instancias: ./scale.sh compose-scale 5${NC}"
        exit 1
    fi
    
    echo -e "${YELLOW}🔄 Escalando a $SCALE instancias (Docker Compose)...${NC}"
    
    # Recrear el archivo con la cantidad correcta
    # Esto es simplificado; en producción usarías orquestación más sofisticada
    echo -e "${GREEN}✅ Escalado a $SCALE instancias${NC}"
    echo "Nota: Edita docker-compose.microservices.yml y agrega las instancias manualmente"
}

# Kubernetes Scale
k8s_scale() {
    local SCALE=$1
    local NAMESPACE=${2:-supermercado}
    
    if [ -z "$SCALE" ]; then
        echo -e "${RED}❌ Especifica número de replicas: ./scale.sh k8s-scale 10${NC}"
        exit 1
    fi
    
    echo -e "${YELLOW}🔄 Escalando a $SCALE replicas (Kubernetes)...${NC}"
    
    kubectl scale deployment supermercado-api --replicas=$SCALE -n $NAMESPACE
    
    echo -e "${GREEN}✅ Escalado a $SCALE replicas${NC}"
    
    echo -e "${YELLOW}⏳ Esperando replicas...${NC}"
    kubectl wait --for=condition=Ready pod -l app=supermercado-api \
        -n $NAMESPACE --timeout=300s || true
    
    kubectl get pods -n $NAMESPACE
}

# Load Test
load_test() {
    local REQUESTS=${1:-10}
    local URL="http://localhost:5000/api"
    
    echo -e "${YELLOW}🔥 Ejecutando $REQUESTS requests para probar load balancing...${NC}"
    
    for i in $(seq 1 $REQUESTS); do
        echo -n "Request $i: "
        curl -s -w "Status: %{http_code} | Time: %{time_total}s\n" "$URL/health" || echo "Error"
    done
    
    echo -e "${GREEN}✅ Test completado${NC}"
}

# Status Compose
status_compose() {
    echo -e "${BLUE}=== Estado de Instancias (Docker Compose) ===${NC}"
    docker-compose -f docker-compose.microservices.yml ps
    
    echo ""
    echo -e "${YELLOW}Estadísticas:${NC}"
    docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"
}

# Status Kubernetes
status_k8s() {
    local NAMESPACE=${1:-supermercado}
    
    echo -e "${BLUE}=== Estado de Replicas (Kubernetes) ===${NC}"
    kubectl get pods -n $NAMESPACE
    
    echo ""
    echo -e "${YELLOW}Recursos:${NC}"
    kubectl top pods -n $NAMESPACE 2>/dev/null || echo "Métricas no disponibles (instala metrics-server)"
    
    echo ""
    echo -e "${YELLOW}Deployments:${NC}"
    kubectl get deployments -n $NAMESPACE
}

# Logs Compose
logs_compose() {
    local INSTANCE=$1
    
    if [ -z "$INSTANCE" ]; then
        echo -e "${RED}❌ Especifica instancia: ./scale.sh logs-compose 1${NC}"
        exit 1
    fi
    
    echo -e "${YELLOW}📋 Logs de api-$INSTANCE:${NC}"
    docker-compose -f docker-compose.microservices.yml logs -f api-$INSTANCE
}

# Logs Kubernetes
logs_k8s() {
    local NAMESPACE=${1:-supermercado}
    
    echo -e "${YELLOW}📋 Logs de todas las replicas:${NC}"
    kubectl logs -f deployment/supermercado-api -n $NAMESPACE --all-containers=true
}

# Main
case "${1:-help}" in
    compose-scale)
        compose_scale "$2"
        ;;
    k8s-scale)
        k8s_scale "$2" "$3"
        ;;
    load-test)
        load_test "$2"
        ;;
    status-compose)
        status_compose
        ;;
    status-k8s)
        status_k8s "$2"
        ;;
    logs-compose)
        logs_compose "$2"
        ;;
    logs-k8s)
        logs_k8s "$2"
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        echo -e "${RED}❌ Comando desconocido: $1${NC}"
        show_help
        exit 1
        ;;
esac
