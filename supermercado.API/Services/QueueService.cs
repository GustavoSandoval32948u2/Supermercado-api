using Azure.Storage.Queues;
using System.Text.Json;
using supermercado.API.Models;

namespace supermercado.API.Services
{
    // Singleton porque QueueClient es thread-safe.
    // Incluye modo simulado para que la demo, Postman y K6 no fallen si Azure/Azurite no esta disponible.
    public class QueueService
    {
        private readonly QueueClient? _queueClient;
        private readonly ILogger<QueueService> _logger;
        private readonly bool _modoSimulado;
        private readonly Queue<string> _mensajesSimulados = new();
        private readonly object _lock = new();

        public QueueService(IConfiguration config, ILogger<QueueService> logger)
        {
            _logger = logger;

            var connectionString = config["AzureStorage:ConnectionString"];
            var queueName = config["AzureStorage:QueueName"] ?? "pedidos-supermercado";
            var usarSimulacionSiFalla = config.GetValue("AzureStorage:UseInMemoryQueueWhenUnavailable", true);

            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("No se configuro AzureStorage:ConnectionString.");

                _queueClient = new QueueClient(connectionString, queueName);
                _queueClient.CreateIfNotExists();

                _logger.LogInformation("Cola '{QueueName}' lista en Azure/Azurite", queueName);
            }
            catch (Exception ex)
            {
                if (!usarSimulacionSiFalla)
                    throw;

                _modoSimulado = true;
                _logger.LogWarning(ex,
                    "No se pudo conectar a Azure Queue Storage. Se activara cola simulada en memoria para demos y pruebas locales.");
            }
        }

        // Serializa el mensaje a JSON y lo manda a Azure Queue Storage o a la cola simulada.
        public async Task EnviarMensajeAsync(MensajePedido mensaje)
        {
            var json = JsonSerializer.Serialize(mensaje);
            var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

            if (_modoSimulado || _queueClient == null)
            {
                lock (_lock)
                {
                    _mensajesSimulados.Enqueue(base64);
                }

                _logger.LogInformation(
                    "[SIMULADO] Mensaje agregado a cola en memoria - VentaId: {VentaId}, Estado: {Estado}",
                    mensaje.VentaId,
                    mensaje.Estado
                );

                await Task.CompletedTask;
                return;
            }

            await _queueClient.SendMessageAsync(base64);

            _logger.LogInformation(
                "Mensaje enviado a la cola - VentaId: {VentaId}, Estado: {Estado}",
                mensaje.VentaId,
                mensaje.Estado
            );
        }

        // Lo usa el worker para leer el siguiente mensaje disponible.
        public async Task<(Azure.Storage.Queues.Models.QueueMessage? mensaje, string? contenidoJson)> RecibirMensajeAsync()
        {
            if (_modoSimulado || _queueClient == null)
            {
                // En modo simulado no consumimos mensajes porque no existe QueueMessage real de Azure.
                // Aun asi, los endpoints de demo pueden enviar mensajes y consultar cantidad pendiente.
                await Task.CompletedTask;
                return (null, null);
            }

            var response = await _queueClient.ReceiveMessageAsync();

            if (response?.Value == null)
                return (null, null);

            var mensaje = response.Value;
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(mensaje.Body.ToString()));

            return (mensaje, json);
        }

        public async Task EliminarMensajeAsync(string messageId, string popReceipt)
        {
            if (_modoSimulado || _queueClient == null)
            {
                await Task.CompletedTask;
                return;
            }

            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }

        public async Task<int> ObtenerCantidadMensajesAsync()
        {
            if (_modoSimulado || _queueClient == null)
            {
                lock (_lock)
                {
                    return _mensajesSimulados.Count;
                }
            }

            var properties = await _queueClient.GetPropertiesAsync();
            return properties.Value.ApproximateMessagesCount;
        }
    }
}
