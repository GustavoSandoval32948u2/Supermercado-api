using Microsoft.AspNetCore.Mvc;
using supermercado.API.Models;
using supermercado.API.Services;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosQueueController : ControllerBase
    {
        private readonly QueueService _queueService;
        private readonly ILogger<PedidosQueueController> _logger;

        public PedidosQueueController(
            QueueService queueService,
            ILogger<PedidosQueueController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        // 🔷 CAMBIAR ESTADO DE PEDIDO
        [HttpPost("cambiar-estado")]
        public async Task<IActionResult> CambiarEstadoPedido(
            [FromBody] CambiarEstadoRequest request)
        {
            if (request == null)
                return BadRequest("Request inválido");

            var estadosValidos = new[]
            {
                "Recibido",
                "Despachado",
                "Entregado"
            };

            if (string.IsNullOrWhiteSpace(request.Estado))
                return BadRequest("Estado requerido");

            if (!estadosValidos.Contains(request.Estado))
            {
                return BadRequest(
                    $"Estado inválido. Los válidos son: {string.Join(", ", estadosValidos)}");
            }

            var mensaje = new MensajePedido
            {
                VentaId = request.VentaId,
                ClienteId = request.ClienteId,
                Estado = request.Estado,
                ClienteNombre = request.ClienteNombre,
                ClienteEmail = request.ClienteEmail,
                Total = request.Total,
                FechaEvento = DateTime.UtcNow,
                DireccionEntrega = request.DireccionEntrega,

                NumeroSeguimiento = request.Estado == "Despachado"
                    ? $"SUP-{DateTime.UtcNow:yyyyMMdd}-{request.VentaId:D4}"
                    : null
            };

            await _queueService.EnviarMensajeAsync(mensaje);

            _logger.LogInformation(
                $"Estado {request.Estado} enviado a la cola para venta {request.VentaId}");

            return Ok(new
            {
                mensaje = $"Estado '{request.Estado}' enviado a la cola para el pedido #{request.VentaId}",
                ventaId = request.VentaId,
                estado = request.Estado,
                timestamp = mensaje.FechaEvento
            });
        }

        // 🔷 SIMULAR FLUJO COMPLETO
        [HttpPost("simular-pedido-completo")]
        public async Task<IActionResult> SimularPedidoCompleto(
            [FromBody] SimularPedidoRequest request)
        {
            if (request == null)
                return BadRequest("Request inválido");

            var ventaId = request.VentaId > 0
                ? request.VentaId
                : int.Parse(DateTime.UtcNow.ToString("HHmmss"));

            var clienteEmail = string.IsNullOrWhiteSpace(request.EmailDestino)
                ? "cliente.prueba@gmail.com"
                : request.EmailDestino;

            var estados = new[]
            {
                "Recibido",
                "Despachado",
                "Entregado"
            };

            var resultados = new List<object>();

            foreach (var estado in estados)
            {
                var msg = new MensajePedido
                {
                    VentaId = ventaId,
                    ClienteId = request.ClienteId,
                    Estado = estado,
                    ClienteNombre = request.ClienteNombre ?? "Cliente de prueba",
                    ClienteEmail = clienteEmail,
                    Total = request.Total > 0
                        ? request.Total
                        : 150.75m,

                    FechaEvento = DateTime.UtcNow,
                    DireccionEntrega = "Zona 10, Ciudad de Guatemala",

                    NumeroSeguimiento = estado == "Despachado"
                        ? $"SUP-{DateTime.UtcNow:yyyyMMdd}-{ventaId:D4}"
                        : null
                };

                await _queueService.EnviarMensajeAsync(msg);

                _logger.LogInformation(
                    $"Mensaje enviado: {estado} para venta {ventaId}");

                resultados.Add(new
                {
                    estado,
                    enviado = true,
                    timestamp = msg.FechaEvento
                });

                await Task.Delay(500);
            }

            return Ok(new
            {
                mensaje = $"Flujo completo simulado para pedido #{ventaId}",
                ventaId,
                clienteEmail,
                estados = resultados
            });
        }

        // 🔷 ESTADO DE LA COLA
        [HttpGet("estado-cola")]
        public async Task<IActionResult> EstadoCola()
        {
            var cantidad =
                await _queueService.ObtenerCantidadMensajesAsync();

            return Ok(new
            {
                mensajesPendientes = cantidad,
                nota = "Cantidad aproximada de mensajes en la cola"
            });
        }

        // 🔷 DATOS DE PRUEBA
        [HttpGet("datos-prueba")]
        public IActionResult ObtenerDatosPrueba()
        {
            return Ok(new
            {
                descripcion = "Datos listos para pruebas en Swagger",

                clientes = new[]
                {
                    new
                    {
                        clienteId = 1,
                        nombre = "Maria Lopez",
                        email = "maria.lopez@gmail.com",
                        total = 89.50m
                    },

                    new
                    {
                        clienteId = 2,
                        nombre = "Carlos Ruiz",
                        email = "carlos.ruiz@hotmail.com",
                        total = 235.00m
                    },

                    new
                    {
                        clienteId = 3,
                        nombre = "Ana Martinez",
                        email = "ana.mtz@gmail.com",
                        total = 52.75m
                    }
                },

                estadosValidos = new[]
                {
                    "Recibido",
                    "Despachado",
                    "Entregado"
                },

                ejemploSimulacion = new
                {
                    ventaId = 1001,
                    clienteId = 1,
                    clienteNombre = "Maria Lopez",
                    emailDestino = "tu-correo@gmail.com",
                    total = 89.50m
                }
            });
        }
    }

    public class CambiarEstadoRequest
    {
        public int VentaId { get; set; }

        public int ClienteId { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string ClienteNombre { get; set; } = string.Empty;

        public string ClienteEmail { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string? DireccionEntrega { get; set; }
    }

    public class SimularPedidoRequest
    {
        public int VentaId { get; set; }

        public int ClienteId { get; set; }

        public string? ClienteNombre { get; set; }

        public string? EmailDestino { get; set; }

        public decimal Total { get; set; }
    }
}