using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.Models;
using supermercado.API.Services;
namespace supermercado.API.Workers
{
    public class PedidoWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly QueueService _queueService;
        private readonly EmailService _emailService;
        private readonly ILogger<PedidoWorker> _logger;

        public PedidoWorker(
            IServiceScopeFactory scopeFactory,
            QueueService queueService,
            EmailService emailService,
            ILogger<PedidoWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _queueService = queueService;
            _emailService = emailService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PedidoWorker iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (mensajeQueue, contenidoJson) =
                        await _queueService.RecibirMensajeAsync();

                    if (string.IsNullOrEmpty(contenidoJson))
                    {
                        await Task.Delay(3000, stoppingToken);
                        continue;
                    }

                    var mensaje =
                        JsonSerializer.Deserialize<MensajePedido>(contenidoJson);

                    if (mensaje == null)
                        continue;

                    var estadosValidos = new[]
{
    "Pedido recibido",
    "Pedido despachado",
    "Pedido entregado",
    "Cancelado",
    "Recibido",
    "Despachado",
    "Entregado"
};

                    if (!estadosValidos.Contains(mensaje.Estado))
                    {
                        _logger.LogWarning("Estado inválido: {Estado}", mensaje.Estado);
                        continue;
                    }

                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var cliente = await context.Clientes.FindAsync(mensaje.ClienteId);

                    if (cliente == null || mensaje.ClienteId == 0)
                    {
                        _logger.LogWarning("Cliente inválido: {ClienteId}", mensaje.ClienteId);
                        continue;
                    }

                    var venta = new Venta
                    {
                        ClienteId = mensaje.ClienteId,
                        Fecha = DateTime.Now,
                        Estado = mensaje.Estado,
                        Total = 0
                    };

                    context.Ventas.Add(venta);
                    await context.SaveChangesAsync(); // genera ID

                    decimal total = 0;

                    foreach (var item in mensaje.Productos)
                    {
                        var producto = await context.Productos.FindAsync(item.ProductoId);

                        if (producto == null)
                        {
                            _logger.LogWarning("Producto no encontrado: {ProductoId}", item.ProductoId);
                            continue;
                        }

                        if (producto.Stock < item.Cantidad)
                        {
                            _logger.LogWarning("Stock insuficiente ProductoId: {ProductoId}", item.ProductoId);
                            continue;
                        }

                        producto.Stock -= item.Cantidad;

                        context.DetalleVentas.Add(new DetalleVenta
                        {
                            VentaId = venta.Id,
                            ProductoId = producto.Id,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = producto.Precio
                        });

                        total += item.Cantidad * producto.Precio;
                    }

                    venta.Total = total;

                    await context.SaveChangesAsync();

                    // actualizar mensaje con datos reales
                    mensaje.VentaId = venta.Id;
                    mensaje.Total = total;

                    // ENVIAR EMAIL
                    await _emailService.EnviarNotificacionPedidoAsync(mensaje);

                    // ELIMINAR MENSAJE
                    if (mensajeQueue != null)
                    {
                        await _queueService.EliminarMensajeAsync(
                            mensajeQueue.MessageId,
                            mensajeQueue.PopReceipt);
                    }

                    _logger.LogInformation(
                        "Pedido procesado correctamente - VentaId: {VentaId} - Estado: {Estado}",
                        venta.Id,
                        venta.Estado);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando pedido async");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}