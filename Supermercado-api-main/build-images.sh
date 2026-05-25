#!/bin/bash

# Dockerfile para compilar en diferentes arquitecturas
# Uso: ./build-images.sh [versión] [arquitectura]

set -e

VERSION=${1:-latest}
ARCH=${2:-amd64}

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}=== Build Supermercado API ===${NC}"
echo "Versión: $VERSION"
echo "Arquitectura: $ARCH"
echo ""

# Validar Docker
if ! command -v docker &> /dev/null; then
    echo -e "${RED}❌ Docker no está instalado${NC}"
    exit 1
fi

# Build desarrollo
echo -e "${YELLOW}🔨 Compilando imagen de desarrollo...${NC}"
docker build \
    --platform linux/$ARCH \
    --build-arg BUILDKIT_INLINE_CACHE=1 \
    -t supermercado-api:dev-$VERSION \
    -f configs/docker/Dockerfile \
    .

echo -e "${GREEN}✅ Imagen desarrollo compilada: supermercado-api:dev-$VERSION${NC}"

# Build producción
echo -e "${YELLOW}🔨 Compilando imagen de producción...${NC}"
docker build \
    --platform linux/$ARCH \
    --build-arg BUILDKIT_INLINE_CACHE=1 \
    -t supermercado-api:$VERSION \
    -f configs/docker/Dockerfile.prod \
    .

echo -e "${GREEN}✅ Imagen producción compilada: supermercado-api:$VERSION${NC}"

# Build multi-arch (requiere buildx)
if command -v docker buildx &> /dev/null; then
    echo -e "${YELLOW}🔨 Compilando para múltiples arquitecturas...${NC}"
    docker buildx build \
        --platform linux/amd64,linux/arm64 \
        -t supermercado-api:$VERSION-multiarch \
        --push \
        -f configs/docker/Dockerfile.prod \
        .
    echo -e "${GREEN}✅ Build multi-arch completado${NC}"
fi

echo ""
echo -e "${GREEN}=== Build completado ===${NC}"
echo "Imágenes disponibles:"
docker images | grep supermercado-api
