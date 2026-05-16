using Azure.Storage.Queues;
using System.Text.Json;
using supermercado.API.Models;

namespace supermercado.API.Services
{
    // singleton porque el QueueClient es thread-safe y no tiene sentido crear uno por request
    public class QueueService
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<QueueService> _logger;

        public QueueService(IConfiguration config, ILogger<QueueService> logger)
        {
            _logger = logger;

            var connectionString = config["AzureStorage:ConnectionString"];
            var queueName = config["AzureStorage:QueueName"] ?? "pedidos-supermercado";

            // si la cola no existe en azure, la crea automaticamente al arrancar
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();

            _logger.LogInformation("Cola '{QueueName}' lista", queueName);
        }

        // serializa el mensaje a json y lo manda a azure en base64
        public async Task EnviarMensajeAsync(MensajePedido mensaje)
        {
            var json = JsonSerializer.Serialize(mensaje);
            var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

            await _queueClient.SendMessageAsync(base64);

            _logger.LogInformation(
                "Mensaje enviado a la cola - VentaId: {VentaId}, Estado: {Estado}",
                mensaje.VentaId,
                mensaje.Estado
            );
        }

        // lo usa el worker para leer el siguiente mensaje disponible
        public async Task<(Azure.Storage.Queues.Models.QueueMessage? mensaje, string? contenidoJson)> RecibirMensajeAsync()
        {
            var response = await _queueClient.ReceiveMessageAsync();

            if (response?.Value == null)
                return (null, null);

            var mensaje = response.Value;
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(mensaje.Body.ToString()));

            return (mensaje, json);
        }

        // hay que borrar el mensaje despues de procesarlo
        // si no, azure lo vuelve a poner disponible despues del visibility timeout
        public async Task EliminarMensajeAsync(string messageId, string popReceipt)
        {
            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }

        // util para ver cuantos mensajes hay en cola sin procesarlos
        public async Task<int> ObtenerCantidadMensajesAsync()
        {
            var properties = await _queueClient.GetPropertiesAsync();
            return properties.Value.ApproximateMessagesCount;
        }
    }
}
