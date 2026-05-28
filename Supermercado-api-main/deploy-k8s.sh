#!/bin/bash

# Script para desplegar en Kubernetes

set -e

NAMESPACE=${1:-supermercado}
ENVIRONMENT=${2:-staging}

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

show_help() {
    echo -e "${BLUE}=== Supermercado API - Deploy a Kubernetes ===${NC}"
    echo ""
    echo "Uso: ./deploy-k8s.sh [namespace] [environment]"
    echo ""
    echo "Parámetros:"
    echo "  namespace    - Namespace de Kubernetes (default: supermercado)"
    echo "  environment  - dev, staging, prod (default: staging)"
    echo ""
    echo "Ejemplos:"
    echo "  ./deploy-k8s.sh supermercado staging"
    echo "  ./deploy-k8s.sh supermercado prod"
    echo ""
}

check_prerequisites() {
    echo -e "${YELLOW}Verificando requisitos...${NC}"
    
    if ! command -v kubectl &> /dev/null; then
        echo -e "${RED}❌ kubectl no está instalado${NC}"
        exit 1
    fi
    
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}❌ Docker no está instalado${NC}"
        exit 1
    fi
    
    # Verificar conexión a cluster
    if ! kubectl cluster-info &> /dev/null; then
        echo -e "${RED}❌ No hay conexión a cluster Kubernetes${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Todos los requisitos cumplidos${NC}"
}

create_namespace() {
    echo -e "${YELLOW}📁 Creando namespace: $NAMESPACE${NC}"
    
    if kubectl get namespace $NAMESPACE &> /dev/null; then
        echo -e "${YELLOW}⚠️  Namespace ya existe${NC}"
    else
        kubectl create namespace $NAMESPACE
        echo -e "${GREEN}✅ Namespace creado${NC}"
    fi
}

build_image() {
    VERSION=$(date +%Y%m%d-%H%M%S)
    REGISTRY=${REGISTRY:-docker.io}  # Cambiar a tu registry
    IMAGE=$REGISTRY/supermercado-api:$VERSION
    
    echo -e "${YELLOW}🔨 Compilando imagen: $IMAGE${NC}"
    docker build -t $IMAGE -f configs/docker/Dockerfile.prod .
    
    echo -e "${YELLOW}📤 Subiendo imagen...${NC}"
    docker push $IMAGE
    
    echo -e "${GREEN}✅ Imagen publicada: $IMAGE${NC}"
    echo $IMAGE
}

deploy_sqlserver() {
    echo -e "${YELLOW}🗄️  Deployando SQL Server...${NC}"
    kubectl apply -f configs/k8s/kubernetes-sqlserver.yml -n $NAMESPACE
    
    echo -e "${YELLOW}⏳ Esperando que SQL Server esté listo...${NC}"
    kubectl wait --for=condition=Ready pod -l app=sqlserver -n $NAMESPACE --timeout=300s
    
    echo -e "${GREEN}✅ SQL Server deplorado${NC}"
}

deploy_api() {
    local IMAGE=$1
    
    echo -e "${YELLOW}📦 Deployando API...${NC}"
    
    # Reemplazar imagen en el manifesto
    kubectl set image deployment/supermercado-api \
        api=$IMAGE \
        -n $NAMESPACE \
        --record || \
    kubectl apply -f configs/k8s/kubernetes-deployment.yml -n $NAMESPACE
    
    echo -e "${YELLOW}⏳ Esperando que API esté lista...${NC}"
    kubectl wait --for=condition=Available deployment/supermercado-api \
        -n $NAMESPACE --timeout=300s || true
    
    echo -e "${GREEN}✅ API deployrada${NC}"
}

show_info() {
    echo ""
    echo -e "${BLUE}=== Información de Despliegue ===${NC}"
    echo ""
    echo "Namespace: $NAMESPACE"
    echo "Ambiente: $ENVIRONMENT"
    echo ""
    echo -e "${YELLOW}Pods:${NC}"
    kubectl get pods -n $NAMESPACE
    echo ""
    echo -e "${YELLOW}Servicios:${NC}"
    kubectl get svc -n $NAMESPACE
    echo ""
    echo -e "${YELLOW}Deployments:${NC}"
    kubectl get deployments -n $NAMESPACE
    echo ""
    echo -e "${YELLOW}Para acceder a la API:${NC}"
    echo "kubectl port-forward svc/supermercado-api-service 5000:80 -n $NAMESPACE"
    echo "http://localhost:5000/swagger"
    echo ""
}

# Main
case "${1:-help}" in
    help|--help|-h)
        show_help
        ;;
    *)
        check_prerequisites
        create_namespace
        
        echo -e "${BLUE}🚀 Iniciando despliegue...${NC}"
        echo ""
        
        deploy_sqlserver
        
        IMAGE=$(build_image)
        deploy_api $IMAGE
        
        show_info
        
        echo -e "${GREEN}✅ Despliegue completado exitosamente${NC}"
        ;;
esac
