#!/bin/bash

# Script para automatizar comandos Docker
# Uso: ./docker-commands.sh [comando]

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

show_help() {
    echo -e "${BLUE}=== Supermercado API - Comandos Docker ===${NC}"
    echo ""
    echo -e "${YELLOW}Uso: ./docker-commands.sh [comando]${NC}"
    echo ""
    echo -e "${GREEN}Comandos disponibles:${NC}"
    echo "  up              - Levantar contenedores (desarrollo)"
    echo "  up-prod         - Levantar contenedores (producción)"
    echo "  down            - Detener contenedores"
    echo "  down-clean      - Detener y eliminar volúmenes"
    echo "  logs            - Ver logs en tiempo real"
    echo "  logs-api        - Ver logs solo de la API"
    echo "  logs-db         - Ver logs solo de SQL Server"
    echo "  restart         - Reiniciar contenedores"
    echo "  ps              - Listar contenedores"
    echo "  build           - Construir imagen Docker"
    echo "  clean           - Limpiar todo (CUIDADO: borra datos)"
    echo "  status          - Ver estado del sistema"
    echo "  db-backup       - Hacer backup de la BD"
    echo "  db-shell        - Conectar a SQL Server"
    echo "  api-shell       - Conectar a shell de la API"
    echo ""
}

case "${1:-help}" in
    up)
        echo -e "${GREEN}🚀 Levantando contenedores...${NC}"
        docker-compose up -d
        echo -e "${GREEN}✅ Contenedores levantados${NC}"
        echo -e "${BLUE}📍 API: http://localhost:5000${NC}"
        echo -e "${BLUE}📊 Swagger: http://localhost:5000/swagger${NC}"
        ;;
    
    up-prod)
        echo -e "${GREEN}🚀 Levantando contenedores (Producción)...${NC}"
        docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
        echo -e "${GREEN}✅ Contenedores levantados${NC}"
        ;;
    
    down)
        echo -e "${YELLOW}⛔ Deteniendo contenedores...${NC}"
        docker-compose down
        echo -e "${GREEN}✅ Contenedores detenidos${NC}"
        ;;
    
    down-clean)
        echo -e "${RED}🗑️  Deteniendo y eliminando volúmenes...${NC}"
        read -p "¿Estás seguro? (S/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Ss]$ ]]; then
            docker-compose down -v
            echo -e "${RED}⚠️  Se eliminaron todos los datos${NC}"
        fi
        ;;
    
    logs)
        docker-compose logs -f
        ;;
    
    logs-api)
        docker-compose logs -f api
        ;;
    
    logs-db)
        docker-compose logs -f sqlserver
        ;;
    
    restart)
        echo -e "${YELLOW}🔄 Reiniciando contenedores...${NC}"
        docker-compose restart
        echo -e "${GREEN}✅ Contenedores reiniciados${NC}"
        ;;
    
    ps)
        docker-compose ps
        ;;
    
    build)
        echo -e "${GREEN}🔨 Construyendo imagen Docker...${NC}"
        docker-compose build
        ;;
    
    clean)
        echo -e "${RED}🧹 Limpiando sistema Docker...${NC}"
        read -p "¿Estás seguro? Esto eliminará todo (S/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Ss]$ ]]; then
            docker-compose down -v
            docker system prune -a --volumes -f
            echo -e "${GREEN}✅ Sistema limpiado${NC}"
        fi
        ;;
    
    status)
        echo -e "${BLUE}=== Estado de Contenedores ===${NC}"
        docker-compose ps
        echo ""
        echo -e "${BLUE}=== Estadísticas ===${NC}"
        docker stats --no-stream
        ;;
    
    db-backup)
        echo -e "${GREEN}💾 Creando backup de la BD...${NC}"
        PASSWORD=$(grep SA_PASSWORD .env.local | cut -d = -f 2)
        docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
            -S localhost -U sa -P "$PASSWORD" \
            -Q "BACKUP DATABASE SupermercadoDB TO DISK = '/var/opt/mssql/backup/backup_$(date +%Y%m%d_%H%M%S).bak'"
        docker cp supermercado-sqlserver:/var/opt/mssql/backup/ ./backups/
        echo -e "${GREEN}✅ Backup completado${NC}"
        ;;
    
    db-shell)
        echo -e "${YELLOW}Conectando a SQL Server...${NC}"
        PASSWORD=$(grep SA_PASSWORD .env.local | cut -d = -f 2)
        docker exec -it supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$PASSWORD"
        ;;
    
    api-shell)
        echo -e "${YELLOW}Conectando a contenedor API...${NC}"
        docker exec -it supermercado-api /bin/bash
        ;;
    
    help|*)
        show_help
        ;;
esac
