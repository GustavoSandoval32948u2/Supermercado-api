using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
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
                    "No se pudo conectar a Azure Queue Storage. Se activara cola simulada en memoria.");
            }
        }

        // =========================
        // ENVIAR MENSAJE
        // =========================
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
                    "[SIMULADO] Mensaje agregado - VentaId: {VentaId}, Estado: {Estado}",
                    mensaje.VentaId,
                    mensaje.Estado
                );

                return;
            }

            await _queueClient.SendMessageAsync(base64);

            _logger.LogInformation(
                "Mensaje enviado a Azure Queue - VentaId: {VentaId}, Estado: {Estado}",
                mensaje.VentaId,
                mensaje.Estado
            );
        }

        // =========================
        // RECIBIR MENSAJE (WORKER)
        // =========================
        public async Task<(QueueMessage? mensaje, string? contenidoJson)> RecibirMensajeAsync()
        {
            // 🔥 MODO SIMULADO
            if (_modoSimulado || _queueClient == null)
            {
                lock (_lock)
                {
                    if (_mensajesSimulados.Count == 0)
                        return (null, null);

                    var mensajeBase64 = _mensajesSimulados.Dequeue();

                    var contenido = System.Text.Encoding.UTF8.GetString(
                        Convert.FromBase64String(mensajeBase64));

                    return (null, contenido);
                }
            }

            // 🔥 MODO AZURE REAL
            var response = await _queueClient.ReceiveMessageAsync();

            if (response?.Value == null)
                return (null, null);

            var mensaje = response.Value;

            var base64 = mensaje.Body.ToString();

            var json = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(base64));

            return (mensaje, json);
        }

        // =========================
        // ELIMINAR MENSAJE
        // =========================
        public async Task EliminarMensajeAsync(string messageId, string popReceipt)
        {
            if (_modoSimulado || _queueClient == null)
                return;

            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }

        // =========================
        // CONTADOR
        // =========================
        public async Task<int> ObtenerCantidadMensajesAsync()
        {
            if (_modoSimulado || _queueClient == null)
            {
                lock (_lock)
                    return _mensajesSimulados.Count;
            }

            var properties = await _queueClient.GetPropertiesAsync();
            return properties.Value.ApproximateMessagesCount;
        }
    }
}