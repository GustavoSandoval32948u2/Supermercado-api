# Documentación Swagger - Supermercado Online

## Ruta principal

```txt
http://localhost:5143/swagger
```

## Endpoints clave para demostrar

### Health Check

```http
GET /health
```

Sirve para validar que la API está levantada.

### Carrito

```http
POST /api/Carrito/agregar/{id}?cantidad=2
GET /api/Carrito
POST /api/Carrito/comprar
```

Estos endpoints funcionan como simulador simple de compra.

### Pedidos y Azure Queue

```http
GET /api/PedidosQueue/datos-prueba
POST /api/PedidosQueue/simular-pedido-completo
GET /api/PedidosQueue/estado-cola
```

Estos endpoints demuestran el procesamiento asíncrono de notificaciones.

### API principal

```http
GET /api/Productos
POST /api/Productos
GET /api/Clientes
POST /api/Clientes
POST /api/Ventas
```

Estos endpoints demuestran la gestión de negocio del supermercado.

## Nota para exposición

En la exposición se puede decir:

> Swagger permite validar y documentar los endpoints de forma visual, facilitando las pruebas funcionales y la entrega técnica del proyecto.
