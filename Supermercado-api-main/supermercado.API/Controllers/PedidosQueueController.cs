using Microsoft.AspNetCore.Mvc;
using supermercado.API.Models;
using supermercado.API.Services;

namespace supermercado.API.Controllers
{
    // controller para interactuar con la cola manualmente desde swagger
    // util para demos y para probar sin tener que hacer ventas reales
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosQueueController : ControllerBase
    {
        private readonly QueueService _queueService;
        private readonly ILogger<PedidosQueueController> _logger;

        public PedidosQueueController(QueueService queueService, ILogger<PedidosQueueController> logger)
        {
            _queueService = queueService;
            _logger = logger;
        }

        // cambia el estado de un pedido y manda el mensaje a la cola
        [HttpPost("cambiar-estado")]
        public async Task<IActionResult> CambiarEstadoPedido([FromBody] CambiarEstadoRequest request)
        {
            var estadosValidos = new[] { "Recibido", "Despachado", "Entregado" };

            if (!estadosValidos.Contains(request.Estado))
                return BadRequest($"Estado invalido. Los validos son: {string.Join(", ", estadosValidos)}");

            var mensaje = new MensajePedido
            {
                VentaId = request.VentaId,
                Estado = request.Estado,
                ClienteNombre = request.ClienteNombre,
                ClienteEmail = request.ClienteEmail,
                Total = request.Total,
                FechaEvento = DateTime.UtcNow,
                DireccionEntrega = request.DireccionEntrega,
                NumeroSeguimiento = request.Estado == "Despachado"
                    ? $"SUP-{DateTime.Now:yyyyMMdd}-{request.VentaId:D4}"
                    : null
            };

            await _queueService.EnviarMensajeAsync(mensaje);

            return Ok(new
            {
                mensaje = $"Estado '{request.Estado}' enviado a la cola para el pedido #{request.VentaId}",
                ventaId = request.VentaId,
                estado = request.Estado,
                timestamp = mensaje.FechaEvento
            });
        }

        // simula el flujo completo de un pedido: manda los 3 estados seguidos
        // ideal para demostrar en clase sin necesidad de hacer ventas reales
        [HttpPost("simular-pedido-completo")]
        public async Task<IActionResult> SimularPedidoCompleto([FromBody] SimularPedidoRequest request)
        {
            var ventaId = request.VentaId > 0 ? request.VentaId : new Random().Next(1000, 9999);
            var clienteEmail = string.IsNullOrEmpty(request.EmailDestino) ? "cliente.prueba@gmail.com" : request.EmailDestino;

            var estados = new[] { "Recibido", "Despachado", "Entregado" };
            var resultados = new List<object>();

            foreach (var estado in estados)
            {
                var msg = new MensajePedido
                {
                    VentaId = ventaId,
                    Estado = estado,
                    ClienteNombre = request.ClienteNombre ?? "Cliente de prueba",
                    ClienteEmail = clienteEmail,
                    Total = request.Total > 0 ? request.Total : 150.75m,
                    FechaEvento = DateTime.UtcNow,
                    DireccionEntrega = "Zona 10, Ciudad de Guatemala",
                    NumeroSeguimiento = estado == "Despachado"
                        ? $"SUP-{DateTime.Now:yyyyMMdd}-{ventaId:D4}"
                        : null
                };

                await _queueService.EnviarMensajeAsync(msg);
                resultados.Add(new { estado, enviado = true, timestamp = msg.FechaEvento });

                // pausa para que no lleguen todos al mismo tiempo
                await Task.Delay(500);
            }

            return Ok(new
            {
                mensaje = $"Flujo completo simulado para pedido #{ventaId}. Se enviaron {estados.Length} mensajes a la cola.",
                ventaId,
                clienteEmail,
                estados = resultados
            });
        }

        // ver cuantos mensajes hay pendientes en la cola
        [HttpGet("estado-cola")]
        public async Task<IActionResult> EstadoCola()
        {
            var cantidad = await _queueService.ObtenerCantidadMensajesAsync();
            return Ok(new { mensajesPendientes = cantidad, nota = "El numero es aproximado segun Azure" });
        }

        // datos de prueba pre-cargados para usar en swagger sin tener que escribir todo
        [HttpGet("datos-prueba")]
        public IActionResult ObtenerDatosPrueba()
        {
            return Ok(new
            {
                descripcion = "Usa estos datos en los otros endpoints para probar",
                clientes = new[]
                {
                    new { nombre = "Maria Lopez",   email = "maria.lopez@gmail.com",   total = 89.50m },
                    new { nombre = "Carlos Ruiz",   email = "carlos.ruiz@hotmail.com", total = 235.00m },
                    new { nombre = "Ana Martinez",  email = "ana.mtz@gmail.com",        total = 52.75m }
                },
                estadosValidos = new[] { "Recibido", "Despachado", "Entregado" },
                ejemploSimulacion = new
                {
                    ventaId = 1001,
                    clienteNombre = "Maria Lopez",
                    emailDestino = "pon.tu.correo@gmail.com",
                    total = 89.50m
                }
            });
        }
    }

    // DTOs del controller, los ponemos aqui para no crear archivos extra
    public class CambiarEstadoRequest
    {
        public int VentaId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string? DireccionEntrega { get; set; }
    }

    public class SimularPedidoRequest
    {
        public int VentaId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? EmailDestino { get; set; }
        public decimal Total { get; set; }
    }
}
