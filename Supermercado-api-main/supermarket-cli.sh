#!/bin/bash

# Script maestro para gestionar todas las versiones
# Uso: ./supermarket-cli.sh [comando] [opciones]

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

VERSION="2.0"
BOLD='\033[1m'

print_banner() {
    clear
    echo -e "${CYAN}"
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║                                                            ║"
    echo "║          🏪 SUPERMERCADO API - CLI MAESTRO v$VERSION                ║"
    echo "║                                                            ║"
    echo "║   Gestión centralizada de todas las versiones             ║"
    echo "║                                                            ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    echo -e "${NC}\n"
}

show_menu() {
    echo -e "${BOLD}┌─ VERSIONES DISPONIBLES ─────────────────────────┐${NC}"
    echo -e "${GREEN}1.${NC} Desarrollo Simple      (1 API)"
    echo -e "${YELLOW}2.${NC} Microservicios        (2-N APIs + Load Balancer)"
    echo -e "${RED}3.${NC} Kubernetes             (Auto-scaling)"
    echo -e "${BLUE}4.${NC} Producción             (Nginx + Reverse Proxy)"
    echo -e "${BOLD}└──────────────────────────────────────────────────┘${NC}"
    echo ""
    echo -e "${BOLD}┌─ COMANDOS ────────────────────────────────────────┐${NC}"
    echo -e "${GREEN}up${NC}              Levantar ambiente seleccionado"
    echo -e "${RED}down${NC}            Detener ambiente"
    echo -e "${YELLOW}logs${NC}           Ver logs"
    echo -e "${BLUE}scale${NC}          Escalar instancias"
    echo -e "${GREEN}status${NC}         Ver estado"
    echo -e "${CYAN}test${NC}           Ejecutar tests"
    echo -e "${YELLOW}build${NC}          Compilar imágenes"
    echo -e "${RED}clean${NC}          Limpiar todo"
    echo -e "${BLUE}help${NC}           Ver ayuda"
    echo -e "${BOLD}└──────────────────────────────────────────────────┘${NC}"
}

select_version() {
    if [ -z "$1" ]; then
        echo -e "${YELLOW}Selecciona versión:${NC}"
        read -p "1-4: " choice
    else
        choice=$1
    fi
    
    case $choice in
        1)
            VERSION_NAME="Desarrollo"
            COMPOSE_FILE="docker-compose.yml"
            ;;
        2)
            VERSION_NAME="Microservicios"
            COMPOSE_FILE="docker-compose.microservices.yml"
            ;;
        3)
            VERSION_NAME="Kubernetes"
            COMPOSE_FILE="kubernetes-deployment.yml"
            ;;
        4)
            VERSION_NAME="Producción"
            COMPOSE_FILE="docker-compose.prod.yml"
            ;;
        *)
            echo -e "${RED}❌ Opción inválida${NC}"
            exit 1
            ;;
    esac
    
    echo -e "${GREEN}✓ Versión seleccionada: $VERSION_NAME${NC}"
}

version_up() {
    local version=$1
    select_version $version
    
    echo -e "${YELLOW}🚀 Levantando $VERSION_NAME...${NC}"
    
    if [ "$COMPOSE_FILE" == "kubernetes-deployment.yml" ]; then
        ./deploy-k8s.sh
    else
        docker-compose -f $COMPOSE_FILE up -d
    fi
    
    echo -e "${GREEN}✅ $VERSION_NAME levantado${NC}"
    print_access_info $version
}

version_down() {
    local version=$1
    select_version $version
    
    echo -e "${YELLOW}⛔ Deteniendo $VERSION_NAME...${NC}"
    
    if [ "$COMPOSE_FILE" == "kubernetes-deployment.yml" ]; then
        kubectl delete namespace supermercado
    else
        docker-compose -f $COMPOSE_FILE down
    fi
    
    echo -e "${GREEN}✅ $VERSION_NAME detenido${NC}"
}

version_logs() {
    local version=$1
    select_version $version
    
    echo -e "${YELLOW}📋 Logs de $VERSION_NAME${NC}"
    
    if [ "$COMPOSE_FILE" == "kubernetes-deployment.yml" ]; then
        kubectl logs -f deployment/supermercado-api -n supermercado
    else
        docker-compose -f $COMPOSE_FILE logs -f
    fi
}

version_status() {
    local version=$1
    select_version $version
    
    echo -e "${BLUE}📊 Estado de $VERSION_NAME${NC}"
    echo ""
    
    if [ "$COMPOSE_FILE" == "kubernetes-deployment.yml" ]; then
        echo -e "${CYAN}Pods:${NC}"
        kubectl get pods -n supermercado
        echo ""
        echo -e "${CYAN}Servicios:${NC}"
        kubectl get svc -n supermercado
    else
        docker-compose -f $COMPOSE_FILE ps
        echo ""
        echo -e "${CYAN}Estadísticas:${NC}"
        docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"
    fi
}

version_scale() {
    local version=$1
    select_version $version
    
    if [ "$VERSION_NAME" == "Microservicios" ]; then
        read -p "¿Cantidad de instancias? " replicas
        ./scale.sh compose-scale $replicas
    elif [ "$VERSION_NAME" == "Kubernetes" ]; then
        read -p "¿Cantidad de replicas? " replicas
        ./scale.sh k8s-scale $replicas
    else
        echo -e "${YELLOW}⚠️  No es escalable en $VERSION_NAME${NC}"
    fi
}

version_test() {
    local version=$1
    select_version $version
    
    echo -e "${YELLOW}🧪 Ejecutando tests para $VERSION_NAME...${NC}"
    echo ""
    
    case $version in
        1)
            echo -e "${CYAN}Test 1: Desarrollo Simple${NC}"
            bash -c 'source TESTING_GUIDE.md' 2>/dev/null || \
            echo "Ver TESTING_GUIDE.md > Test 1"
            ;;
        2)
            echo -e "${CYAN}Test 2: Microservicios${NC}"
            bash -c 'source TESTING_GUIDE.md' 2>/dev/null || \
            echo "Ver TESTING_GUIDE.md > Test 2"
            ;;
        3)
            echo -e "${CYAN}Test 3: Kubernetes${NC}"
            bash -c 'source TESTING_GUIDE.md' 2>/dev/null || \
            echo "Ver TESTING_GUIDE.md > Test 3"
            ;;
        4)
            echo -e "${CYAN}Test Producción${NC}"
            echo "Ver TESTING_GUIDE.md > Test de Performance"
            ;;
    esac
}

print_access_info() {
    local version=$1
    echo ""
    echo -e "${BLUE}═══════════════════════════════════════════════════${NC}"
    
    case $version in
        1)
            echo -e "${GREEN}API:${NC} http://localhost:5000"
            echo -e "${GREEN}Swagger:${NC} http://localhost:5000/swagger"
            echo -e "${GREEN}SQL Server:${NC} localhost:1433"
            ;;
        2)
            echo -e "${GREEN}API (Load Balancer):${NC} http://localhost:5000"
            echo -e "${GREEN}Swagger:${NC} http://localhost:5000/swagger"
            echo -e "${GREEN}SQL Server:${NC} localhost:1433"
            echo -e "${YELLOW}Instancias:${NC} api-1, api-2 (+ más si escalaste)"
            ;;
        3)
            echo -e "${GREEN}Acceso:${NC} kubectl port-forward svc/supermercado-api-service 5000:80 -n supermercado"
            echo -e "${GREEN}API:${NC} http://localhost:5000"
            echo -e "${GREEN}Swagger:${NC} http://localhost:5000/swagger"
            echo -e "${YELLOW}Replicas:${NC} 2-10 (auto-scaling)"
            ;;
        4)
            echo -e "${GREEN}API (Nginx Proxy):${NC} http://localhost:5000"
            echo -e "${GREEN}Swagger:${NC} http://localhost:5000/swagger"
            echo -e "${YELLOW}SSL/TLS:${NC} Configurar certificados en certs/"
            ;;
    esac
    
    echo -e "${BLUE}═══════════════════════════════════════════════════${NC}"
}

show_help() {
    print_banner
    show_menu
    echo ""
    echo -e "${BOLD}EJEMPLOS DE USO:${NC}"
    echo ""
    echo -e "  ${CYAN}# Desarrollo${NC}"
    echo -e "  ./supermarket-cli.sh up 1"
    echo -e "  ./supermarket-cli.sh logs 1"
    echo -e "  ./supermarket-cli.sh down 1"
    echo ""
    echo -e "  ${CYAN}# Microservicios${NC}"
    echo -e "  ./supermarket-cli.sh up 2"
    echo -e "  ./supermarket-cli.sh scale 2"
    echo ""
    echo -e "  ${CYAN}# Kubernetes${NC}"
    echo -e "  ./supermarket-cli.sh up 3"
    echo -e "  ./supermarket-cli.sh scale 3"
    echo ""
    echo -e "  ${CYAN}# Tests${NC}"
    echo -e "  ./supermarket-cli.sh test 1"
    echo ""
}

# Main
print_banner

if [ $# -eq 0 ]; then
    show_help
    exit 0
fi

COMMAND=$1
VERSION_ARG=$2

case $COMMAND in
    up)
        version_up $VERSION_ARG
        ;;
    down)
        version_down $VERSION_ARG
        ;;
    logs)
        version_logs $VERSION_ARG
        ;;
    status)
        version_status $VERSION_ARG
        ;;
    scale)
        version_scale $VERSION_ARG
        ;;
    test)
        version_test $VERSION_ARG
        ;;
    build)
        read -p "Versión (1-4): " v
        select_version $v
        echo -e "${YELLOW}🔨 Compilando $VERSION_NAME...${NC}"
        if [ "$COMPOSE_FILE" != "kubernetes-deployment.yml" ]; then
            docker-compose -f $COMPOSE_FILE build
        fi
        ;;
    clean)
        echo -e "${RED}⚠️  Esto eliminará TODOS los contenedores y volúmenes${NC}"
        read -p "¿Estás seguro? (S/N): " confirm
        if [ "$confirm" = "S" ] || [ "$confirm" = "s" ]; then
            docker-compose down -v
            docker-compose -f docker-compose.microservices.yml down -v
            docker-compose -f docker-compose.prod.yml down -v
            kubectl delete namespace supermercado 2>/dev/null || true
            docker system prune -a --volumes -f
            echo -e "${GREEN}✅ Sistema limpiado${NC}"
        fi
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        echo -e "${RED}❌ Comando desconocido: $COMMAND${NC}"
        show_help
        exit 1
        ;;
esac
