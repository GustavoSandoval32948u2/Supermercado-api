# Azure Queue Storage - Supermercado API

Esta parte del proyecto corresponde a la implementacion del sistema de mensajeria asincrona
usando Azure Queue Storage. La idea es que cada vez que un pedido cambia de estado,
se manda un mensaje a una cola en Azure y un proceso en segundo plano lo lee y le envia
un correo al cliente avisandole.

---

## Que se implemento

Se agregaron los siguientes archivos al proyecto sin modificar el codigo existente de los demas:

**Services/QueueService.cs**
Maneja toda la comunicacion con Azure Queue Storage. Tiene metodos para enviar mensajes a la cola,
recibirlos y eliminarlos una vez procesados. Los mensajes viajan en formato JSON codificado en base64,
que es lo que Azure espera por defecto.

**Services/EmailService.cs**
Se encarga de enviar los correos al cliente usando SMTP de Gmail. Arma un correo en HTML
con el detalle del pedido y lo manda dependiendo del estado: Recibido, Despachado o Entregado.
Cada estado tiene su propio asunto y color en el correo para que sea claro.

**BackgroundServices/QueueConsumerWorker.cs**
Es un worker que corre en segundo plano mientras la API esta levantada. Cada 10 segundos
revisa si hay mensajes en la cola, los procesa y manda el correo correspondiente.
Si el correo falla por alguna razon, el mensaje no se elimina de la cola para que Azure
lo reintente automaticamente despues.

**Models/MensajePedido.cs**
Es el objeto que viaja dentro de la cola. Tiene los datos del pedido y del cliente
que necesita el worker para armar y enviar el correo.

**Controllers/PedidosQueueController.cs**
Expone endpoints en Swagger para interactuar con la cola manualmente. Sirve para probar
el flujo sin necesidad de crear ventas reales. Tiene un endpoint que simula los tres
estados de un pedido de una sola vez.

Los dos unicos archivos del proyecto original que se tocaron fueron:

- **Program.cs**: se agregaron tres lineas para registrar los nuevos servicios.
- **appsettings.json**: se agregaron las secciones de configuracion de Azure y email al final.
Todo lo demas quedo exactamente igual.

---

## Configuracion antes de correr

### 1. Azure Storage Queue

Primero hay que tener un Storage Account creado en Azure Portal (portal.azure.com).

1. Buscar "Storage accounts" y entrar al que se creo para el proyecto.
2. En el menu lateral ir a "Security + networking" y luego "Access keys".
3. Copiar la connection string completa (la que empieza con `DefaultEndpointsProtocol=https...`).
4. Pegarla en `appsettings.json` en el campo `AzureStorage:ConnectionString`.

La cola `pedidos-supermercado` se crea automaticamente la primera vez que corre la API,
no hay que crearla manualmente en Azure.

### 2. Gmail para el envio de correos

Gmail ya no permite usar la contrasena normal para apps externas, hay que generar
una App Password especifica:

1. Entrar a la cuenta de Google que se va a usar para enviar.
2. Ir a Seguridad y activar la verificacion en dos pasos si no esta activada.
3. Buscar "Contrasenas de aplicaciones" (App Passwords).
4. Crear una nueva para "Correo" y copiar la clave de 16 caracteres que genera Google.
5. Pegar esa clave en `appsettings.json` en `Email:Password`.
6. El correo de esa cuenta va en `Email:Remitente`.

El `appsettings.json` deberia quedar asi en esas secciones:

```json
"AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
    "QueueName": "pedidos-supermercado"
},
"Email": {
    "Remitente": "tucorreo@gmail.com",
    "Password": "abcd efgh ijkl mnop",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587"
}
```

---

## Como probarlo desde Swagger

Con la API corriendo, entrar a `https://localhost:{puerto}/swagger`.

Los endpoints del `PedidosQueueController` son:

**GET /api/PedidosQueue/datos-prueba**
Muestra clientes y valores de ejemplo para copiar y pegar en los otros endpoints.

**GET /api/PedidosQueue/estado-cola**
Muestra cuantos mensajes hay pendientes en la cola en ese momento.

**POST /api/PedidosQueue/cambiar-estado**
Manda un mensaje con el estado que se elija. Ejemplo del body:

```json
{
  "ventaId": 1001,
  "estado": "Recibido",
  "clienteNombre": "Maria Lopez",
  "clienteEmail": "tucorreo@gmail.com",
  "total": 89.50,
  "direccionEntrega": "Zona 10, Guatemala"
}
```

**POST /api/PedidosQueue/simular-pedido-completo**
Manda los tres estados seguidos (Recibido, Despachado, Entregado) automaticamente.
Es el mas util para demostrar el flujo completo de una vez. Ejemplo:

```json
{
  "ventaId": 1001,
  "clienteNombre": "Maria Lopez",
  "emailDestino": "tucorreo@gmail.com",
  "total": 89.50
}
```

---

## Como funciona el flujo completo

1. Se llama al endpoint de cambiar estado o simular pedido.
2. El mensaje se serializa a JSON, se codifica en base64 y se manda a Azure Queue Storage.
3. El worker que corre en background revisa la cola cada 10 segundos.
4. Cuando encuentra un mensaje, lo decodifica, construye el correo HTML y lo manda al cliente.
5. Si el envio fue exitoso, el mensaje se elimina de la cola.
6. Si algo falla, el mensaje permanece en la cola y Azure lo vuelve a poner disponible
   despues de un tiempo para que se reintente.

---

## Paquetes utilizados

No se agrego ningun paquete nuevo al proyecto. El SDK de Azure que se usa
(`Azure.Storage.Queues 12.25.0`) ya estaba incluido en el `.csproj` del proyecto original.
El envio de correos usa `System.Net.Mail`, que es parte del framework de .NET y no requiere
instalacion adicional.
