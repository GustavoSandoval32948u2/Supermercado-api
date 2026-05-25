#!/bin/bash

# Makefile para Supermercado API
# Uso: make [comando]

.PHONY: help build up down logs clean restart test

help:
	@echo "=== Supermercado API - Comandos Docker ==="
	@echo ""
	@echo "make build          - Construir imagen Docker"
	@echo "make up             - Levantar contenedores (desarrollo)"
	@echo "make up-prod        - Levantar contenedores (producción)"
	@echo "make down           - Detener contenedores"
	@echo "make down-clean     - Detener y eliminar volúmenes"
	@echo "make logs           - Ver logs en tiempo real"
	@echo "make logs-api       - Ver logs solo de la API"
	@echo "make logs-db        - Ver logs solo de SQL Server"
	@echo "make restart        - Reiniciar contenedores"
	@echo "make ps             - Listar contenedores"
	@echo "make db-backup      - Hacer backup de la BD"
	@echo "make db-shell       - Conectar a shell de SQL Server"
	@echo "make api-shell      - Conectar a shell del contenedor API"
	@echo "make clean          - Limpiar todo (CUIDADO: borra datos)"
	@echo "make status         - Ver estado del sistema"
	@echo ""

build:
	@echo "🔨 Construyendo imagen Docker..."
	docker-compose build

build-prod:
	@echo "🔨 Construyendo imagen para producción..."
	docker-compose -f docker-compose.prod.yml build

up:
	@echo "🚀 Levantando contenedores (Desarrollo)..."
	docker-compose up -d
	@echo "✅ Contenedores levantados"
	@echo "📍 API: http://localhost:5000"
	@echo "📊 Swagger: http://localhost:5000/swagger"
	@echo "🗄️  SQL Server: localhost:1433"

up-prod:
	@echo "🚀 Levantando contenedores (Producción)..."
	docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
	@echo "✅ Contenedores levantados"

down:
	@echo "⛔ Deteniendo contenedores..."
	docker-compose down

down-clean:
	@echo "🗑️  Deteniendo y eliminando volúmenes..."
	docker-compose down -v
	@echo "⚠️  Se eliminaron todos los datos"

restart:
	@echo "🔄 Reiniciando contenedores..."
	docker-compose restart
	@echo "✅ Contenedores reiniciados"

logs:
	@docker-compose logs -f

logs-api:
	@docker-compose logs -f api

logs-db:
	@docker-compose logs -f sqlserver

ps:
	@docker-compose ps

status:
	@echo "=== Estado del Sistema ==="
	@docker-compose ps
	@echo ""
	@echo "=== Estadísticas ==="
	@docker stats --no-stream

db-backup:
	@echo "💾 Creando backup de la BD..."
	@docker exec supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
		-S localhost -U sa -P $$(grep SA_PASSWORD .env.local | cut -d = -f 2) \
		-Q "BACKUP DATABASE SupermercadoDB TO DISK = '/var/opt/mssql/backup/backup_$$(date +%Y%m%d_%H%M%S).bak'"
	@docker cp supermercado-sqlserver:/var/opt/mssql/backup/ ./backups/
	@echo "✅ Backup completado"

db-shell:
	@docker exec -it supermercado-sqlserver /opt/mssql-tools/bin/sqlcmd \
		-S localhost -U sa -P $$(grep SA_PASSWORD .env.local | cut -d = -f 2)

api-shell:
	@docker exec -it supermercado-api /bin/bash

clean:
	@echo "🧹 Limpiando sistema Docker..."
	@docker-compose down -v
	@docker system prune -a --volumes -f
	@echo "✅ Sistema limpiado"

# Tareas de desarrollo
env-setup:
	@echo "📝 Configurando variables de entorno..."
	@cp .env.example .env.local
	@echo "✅ Archivo .env.local creado. Por favor, edítalo según tus necesidades."

test:
	@echo "🧪 Ejecutando pruebas..."
	@docker-compose exec api dotnet test

format:
	@echo "📋 Formateando código..."
	@docker-compose exec api dotnet format

lint:
	@echo "🔍 Analizando código..."
	@docker-compose exec api dotnet build --no-restore -v q

version:
	@echo "Docker version:"
	@docker --version
	@echo "Docker Compose version:"
	@docker-compose --version
