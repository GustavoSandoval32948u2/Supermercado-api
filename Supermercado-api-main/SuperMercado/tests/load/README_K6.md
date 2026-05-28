# Prueba de carga con K6 - Héctor Jesus

## Requisito
Tener la API ejecutándose, por ejemplo:

```bash
dotnet run --project supermercado.API/supermercado.API.csproj
```

## Ejecutar prueba

```bash
k6 run tests/load/k6-supermercado-hector.js
```

Si la API usa otro puerto:

```bash
BASE_URL=http://localhost:7239 k6 run tests/load/k6-supermercado-hector.js
```

## Qué valida

- `/health`
- Agregar producto al carrito
- Consultar carrito
- Confirmar compra
- Simular pedido completo con cola

## Evidencia esperada

Guardar captura donde se vea:

- `checks`: cercano a 100%
- `http_req_failed`: menor a 5%
- `http_req_duration`: percentil 95 menor a 1200 ms
