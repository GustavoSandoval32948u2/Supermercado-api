#!/bin/bash
set -e

echo "🚀 Iniciando Supermercado API..."
echo "📦 Ambiente: $ASPNETCORE_ENVIRONMENT"

# Ejecutar aplicación
exec dotnet supermercado.API.dll
