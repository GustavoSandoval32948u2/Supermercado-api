using supermercado.API.Models;
using supermercado.API.Services;
using System.Text.Json;

namespace supermercado.API.BackgroundServices
{
    // este worker corre en segundo plano mientras la api este levantada
    // revisa la cola cada 10 segundos y procesa los mensajes que encuentre
    public class QueueConsumerWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<QueueConsumerWorker> _logger;
        private readonly int _intervaloSegundos = 10;

        public QueueConsumerWorker(IServiceProvider serviceProvider, ILogger<QueueConsumerWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consumidor de cola iniciado, revisando cada {Segundos}s", _intervaloSegundos);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcesarMensajesPendientes();
                }
                catch (Exception ex)
                {
                    // si explota algo general no queremos que muera el worker
                    _logger.LogError(ex, "Error inesperado en el consumidor de cola");
                }

                await Task.Delay(TimeSpan.FromSeconds(_intervaloSegundos), stoppingToken);
            }

            _logger.LogInformation("Consumidor de cola detenido");
        }

        private async Task ProcesarMensajesPendientes()
        {
            // creamos un scope para resolver los servicios correctamente
            using var scope = _serviceProvider.CreateScope();
            var queueService = scope.ServiceProvider.GetRequiredService<QueueService>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            // procesamos todos los mensajes disponibles en esta ronda
            while (true)
            {
                var (queueMessage, json) = await queueService.RecibirMensajeAsync();

                if (queueMessage == null || json == null)
                    break; // no habia nada, esperamos al siguiente ciclo

                MensajePedido? mensajePedido = null;

                try
                {
                    mensajePedido = JsonSerializer.Deserialize<MensajePedido>(json);

                    if (mensajePedido == null)
                    {
                        _logger.LogWarning("Mensaje deserializado como null, se descarta");
                        await queueService.EliminarMensajeAsync(queueMessage.MessageId, queueMessage.PopReceipt);
                        continue;
                    }

                    _logger.LogInformation(
                        "Procesando - VentaId: {VentaId}, Estado: {Estado}, Email: {Email}",
                        mensajePedido.VentaId,
                        mensajePedido.Estado,
                        mensajePedido.ClienteEmail
                    );

                    await emailService.EnviarNotificacionPedidoAsync(mensajePedido);

                    // borramos el mensaje solo si todo salio bien
                    await queueService.EliminarMensajeAsync(queueMessage.MessageId, queueMessage.PopReceipt);

                    _logger.LogInformation("Mensaje procesado y eliminado - VentaId: {VentaId}", mensajePedido.VentaId);
                }
                catch (JsonException ex)
                {
                    // json malo, lo descartamos para que no quede en loop infinito
                    _logger.LogError(ex, "Mensaje con formato invalido, se descarta");
                    await queueService.EliminarMensajeAsync(queueMessage.MessageId, queueMessage.PopReceipt);
                }
                catch (Exception ex)
                {
                    // si fallo el envio, no borramos el mensaje
                    // azure lo va a volver a poner disponible despues del visibility timeout
                    _logger.LogError(ex, "Error procesando VentaId: {VentaId}", mensajePedido?.VentaId);
                    break;
                }
            }
        }
    }
}
