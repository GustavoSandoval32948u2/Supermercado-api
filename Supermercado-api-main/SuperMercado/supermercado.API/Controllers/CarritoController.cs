using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.DTOs;
using supermercado.API.Models;
using supermercado.API.Services;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QueueService _queueService;

        public CarritoController(AppDbContext context, QueueService queueService)
        {
            _context = context;
            _queueService = queueService;
        }

        // =========================
        // CARROS POR CLIENTE (MEMORIA)
        // =========================
        private static readonly Dictionary<int, List<CarritoItem>> carritos = new();
        private static readonly object _lock = new();

        private List<CarritoItem> ObtenerCarrito(int clienteId)
        {
            lock (_lock)
            {
                if (!carritos.ContainsKey(clienteId))
                    carritos[clienteId] = new List<CarritoItem>();

                return carritos[clienteId];
            }
        }

        // =========================
        // AGREGAR PRODUCTO
        // =========================
        [HttpPost("agregar/{id}")]
        public async Task<IActionResult> AgregarProducto(int id, int clienteId, int cantidad = 1)
        {
            if (cantidad <= 0)
                return BadRequest("Cantidad inválida");

            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound("Producto no encontrado");

            var carrito = ObtenerCarrito(clienteId);

            var existente = carrito.FirstOrDefault(c => c.ProductoId == id);

            int nuevaCantidad = (existente?.Cantidad ?? 0) + cantidad;

            if (producto.Stock < nuevaCantidad)
                return BadRequest("Stock insuficiente");

            if (existente != null)
            {
                existente.Cantidad = nuevaCantidad;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Cantidad = cantidad,
                    Precio = producto.Precio
                });
            }

            return Ok(new
            {
                mensaje = "Producto agregado al carrito",
                carrito
            });
        }

        // =========================
        // VER CARRITO
        // =========================
        [HttpGet("{clienteId}")]
        public IActionResult VerCarrito(int clienteId)
        {
            var carrito = ObtenerCarrito(clienteId);

            var total = carrito.Sum(i => i.Precio * i.Cantidad);

            return Ok(new
            {
                productos = carrito,
                total
            });
        }

        // =========================
        // LIMPIAR CARRITO
        // =========================
        [HttpDelete("limpiar/{clienteId}")]
        public IActionResult LimpiarCarrito(int clienteId)
        {
            ObtenerCarrito(clienteId).Clear();

            return Ok(new
            {
                mensaje = "Carrito limpiado correctamente"
            });
        }

        [HttpPost("comprar")]
        public async Task<IActionResult> Comprar(int clienteId)
        {
            List<CarritoItem> carritoSnapshot;

            lock (_lock)
            {
                var carrito = ObtenerCarrito(clienteId);

                if (carrito.Count == 0)
                    return BadRequest("El carrito está vacío");

                // 🔥 COPIA SEGURA
                carritoSnapshot = carrito
                    .Select(c => new CarritoItem
                    {
                        ProductoId = c.ProductoId,
                        Nombre = c.Nombre,
                        Cantidad = c.Cantidad,
                        Precio = c.Precio
                    })
                    .ToList();

                carrito.Clear();
            }

            var cliente = await _context.Clientes.FindAsync(clienteId);

            if (cliente == null)
                return NotFound("Cliente no encontrado");

            var venta = new Venta
            {
                ClienteId = clienteId,
                Fecha = DateTime.Now,
                Estado = "Pedido recibido",
                Total = 0,
                Detalles = new List<DetalleVenta>()
            };

            decimal total = 0;

            foreach (var item in carritoSnapshot)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);

                if (producto == null)
                    return NotFound($"Producto {item.ProductoId} no encontrado");

                if (producto.Stock < item.Cantidad)
                    return BadRequest($"Stock insuficiente para {producto.Nombre}");

                producto.Stock -= item.Cantidad;

                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio
                });

                total += item.Cantidad * producto.Precio;
            }

            venta.Total = total;

            _context.Ventas.Add(venta);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Compra realizada correctamente",
                ventaId = venta.Id,
                total
            });
        }

        [HttpPost("comprar-async")]
        public async Task<IActionResult> ComprarAsync(int clienteId)
        {
            List<CarritoItem> carritoSnapshot;

            lock (_lock)
            {
                var carrito = ObtenerCarrito(clienteId);

                if (carrito.Count == 0)
                    return BadRequest("El carrito está vacío");

                // 🔥 COPIA SEGURA
                carritoSnapshot = carrito
                    .Select(c => new CarritoItem
                    {
                        ProductoId = c.ProductoId,
                        Nombre = c.Nombre,
                        Cantidad = c.Cantidad,
                        Precio = c.Precio
                    })
                    .ToList();

                carrito.Clear();
            }

            var cliente = await _context.Clientes.FindAsync(clienteId);

            if (cliente == null)
                return NotFound("Cliente no encontrado");

            var mensaje = new MensajePedido
            {
                ClienteId = cliente.Id,
                ClienteNombre = cliente.Nombre,
                ClienteEmail = cliente.Email,
                Productos = carritoSnapshot.Select(p => new DetalleVentaDTO
                {
                    ProductoId = p.ProductoId,
                    Cantidad = p.Cantidad
                }).ToList(),
                Estado = "Pedido recibido",
                FechaEvento = DateTime.UtcNow
            };

            await _queueService.EnviarMensajeAsync(mensaje);

            return Accepted(new
            {
                mensaje = "Pedido enviado correctamente",
                cliente = cliente.Nombre
            });
        }
    }
}