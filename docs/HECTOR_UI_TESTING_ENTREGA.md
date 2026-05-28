# Héctor Jesus - UI, Testing y Entrega

## Resumen

Esta carpeta agrega la parte correspondiente a **Héctor Jesus** dentro del proyecto de Supermercado Online.

Responsabilidades cubiertas:

- Interfaz simple por consola.
- Simulador de compras.
- Pruebas funcionales con Postman.
- Pruebas automatizadas con xUnit.
- Pruebas de carga con K6.
- Documentación Swagger.
- Reglas de entrega y organización para GitHub.

---

## 1. Interfaz simple / simulador de compras

Proyecto agregado:

```txt
Supermercado.SimuladorCompras/
```

Permite demostrar desde consola:

1. Ver productos de supermercado con datos coherentes.
2. Agregar productos al carrito.
3. Ver carrito.
4. Confirmar compra.
5. Simular pedido completo.
6. Ver estado de la cola.

Comando:

```bash
dotnet run --project Supermercado.SimuladorCompras/Supermercado.SimuladorCompras.csproj
```

Si la API corre en otro puerto:

```bash
dotnet run --project Supermercado.SimuladorCompras/Supermercado.SimuladorCompras.csproj -- http://localhost:7239
```

---

## 2. Swagger UI

Se mejoró `Program.cs` para agregar:

- Título del proyecto.
- Versión.
- Descripción.
- Contacto de la parte de Héctor.
- Ruta `/swagger`.
- Ruta `/health`.

Ruta esperada:

```txt
http://localhost:5143/swagger
```

También se agregó configuración en `appsettings.json`:

```json
"Swagger": {
  "Enabled": true
}
```

---

## 3. Pruebas funcionales con Postman

Archivos agregados:

```txt
tests/postman/Supermercado_Hector.postman_collection.json
tests/postman/Supermercado_Hector.postman_environment.json
```

La colección prueba:

- Health Check.
- Swagger JSON.
- Datos de prueba de pedidos.
- Agregar producto al carrito.
- Ver carrito.
- Confirmar compra.
- Simular pedido completo con cola.
- Estado de cola.
- Crear producto real.
- Crear cliente real.

Para usarla:

1. Abrir Postman.
2. Importar la colección.
3. Importar el environment.
4. Revisar que `baseUrl` sea `http://localhost:5143`.
5. Ejecutar la colección completa.

---

## 4. Pruebas xUnit

Proyecto agregado:

```txt
supermercado.API.Tests/
```

Incluye pruebas para:

- Productos.
- Clientes.
- Ventas.
- Stock insuficiente.
- Carrito.

Comando:

```bash
dotnet test supermercado.API.Tests/supermercado.API.Tests.csproj
```

También se puede correr desde la solución:

```bash
dotnet test supermercado.API.sln
```

---

## 5. Pruebas de carga con K6

Archivo agregado:

```txt
tests/load/k6-supermercado-hector.js
```

Comando:

```bash
k6 run tests/load/k6-supermercado-hector.js
```

Si la API corre en otro puerto:

```bash
BASE_URL=http://localhost:7239 k6 run tests/load/k6-supermercado-hector.js
```

La prueba simula varios usuarios consultando y comprando en la API.

---

## 6. Azure Queue Storage / modo demo

Se actualizó `QueueService.cs` para permitir un **modo simulado en memoria** cuando Azure Queue Storage o Azurite no estén disponibles.

Esto evita que la demo falle por falta de configuración de Azure.

Configuración agregada:

```json
"AzureStorage": {
  "ConnectionString": "UseDevelopmentStorage=true",
  "QueueName": "pedidos-supermercado",
  "UseInMemoryQueueWhenUnavailable": true
}
```

En producción, se debe reemplazar la connection string por la real de Azure.

---

## 7. GitHub y entrega

Antes de subir a GitHub:

```bash
git status
git add .
git commit -m "Agregar modulo de UI testing y entrega de Hector"
git push
```

Evidencias recomendadas para entregar:

- Captura de Swagger abierto.
- Captura del simulador de compras.
- Captura de Postman con tests en verde.
- Captura de `dotnet test` exitoso.
- Captura de K6 con resultados.
- Link del repositorio GitHub.

---

## Checklist de Héctor

| Requisito | Estado esperado |
|---|---|
| Interfaz simple | Cubierto con consola |
| Simulador de compras | Cubierto |
| Postman | Cubierto |
| xUnit | Cubierto |
| K6 | Cubierto |
| Swagger UI | Mejorado |
| Health Check | Agregado |
| GitHub | Documentado |
| Datos coherentes | Cubierto con productos/clientes reales de ejemplo |
