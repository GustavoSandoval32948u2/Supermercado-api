# Paquete Héctor Jesus - UI, Testing y Entrega

Este proyecto fue preparado sobre el ZIP original del supermercado y agrega los archivos necesarios para cubrir la parte de **Héctor Jesus**.

## Qué se agregó

```txt
Supermercado.SimuladorCompras/          Interfaz de consola y simulador de compras
supermercado.API.Tests/                 Pruebas xUnit
tests/postman/                          Colección Postman y environment
tests/load/                             Prueba de carga K6
docs/HECTOR_UI_TESTING_ENTREGA.md       Documentación de la parte de Héctor
docs/SWAGGER_DOCUMENTACION_HECTOR.md    Guía de Swagger
scripts/hector-demo.ps1                 Comandos rápidos para demo
```

También se mejoró:

```txt
supermercado.API/Program.cs             Swagger con título/descripción y /health
supermercado.API/Services/QueueService.cs  Modo demo en memoria para Azure Queue
supermercado.API/appsettings.json       Swagger y AzureStorage configurados
supermercado.API/supermercado.API.csproj XML documentation activada
supermercado.API.sln                    Proyectos agregados a la solución
```

## Cómo probar

### 1. Ejecutar API

```bash
dotnet run --project supermercado.API/supermercado.API.csproj
```

Swagger:

```txt
http://localhost:5143/swagger
```

Health:

```txt
http://localhost:5143/health
```

### 2. Ejecutar simulador de compras

```bash
dotnet run --project Supermercado.SimuladorCompras/Supermercado.SimuladorCompras.csproj
```

### 3. Ejecutar pruebas xUnit

```bash
dotnet test supermercado.API.Tests/supermercado.API.Tests.csproj
```

### 4. Ejecutar K6

```bash
k6 run tests/load/k6-supermercado-hector.js
```

### 5. Postman

Importar:

```txt
tests/postman/Supermercado_Hector.postman_collection.json
tests/postman/Supermercado_Hector.postman_environment.json
```

## Entrega recomendada

Para la evidencia, tomar capturas de:

1. Swagger abierto.
2. Simulador de consola haciendo una compra.
3. Postman con pruebas exitosas.
4. `dotnet test` exitoso.
5. K6 finalizado.
6. Proyecto subido a GitHub.
