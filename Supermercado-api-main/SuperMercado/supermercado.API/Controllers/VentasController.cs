// =========================
// VentasController (ORIGINAL + COMPATIBLE TESTS)
// =========================
using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QueueService _queueService;

        public VentasController(AppDbContext context, QueueService queueService)
        {
            _context = context;
            _queueService = queueService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearVenta(VentaDTO dto)
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);

            if (cliente == null)
                return NotFound("Cliente no existe");

            if (dto.Productos == null || !dto.Productos.Any())
                return BadRequest("Debe agregar productos");

            var venta = new Venta
            {
                ClienteId = dto.ClienteId,
                Fecha = DateTime.Now,
                Estado = "Pedido recibido",
                Total = 0
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            decimal total = 0;

            foreach (var item in dto.Productos)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);

                if (producto == null)
                    return NotFound($"Producto {item.ProductoId} no existe");

                if (producto.Stock < item.Cantidad)
                    return BadRequest($"Stock insuficiente para {producto.Nombre}");

                producto.Stock -= item.Cantidad;

                _context.DetalleVentas.Add(new DetalleVenta
                {
                    VentaId = venta.Id,
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio
                });

                total += item.Cantidad * producto.Precio;
            }

            venta.Total = total;
            await _context.SaveChangesAsync();

            // 🔥 FIX TEST: debe existir "total"
            return Ok(new
            {
                total,
                id = venta.Id
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .Include(v => v.Cliente)
                .AsNoTracking()
                .ToListAsync();

            // 🔥 FIX TEST: debe existir "data"
            return Ok(new
            {
                data = ventas
            });
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound("Venta no encontrada");

            var estadosValidos = new[]
            {
                "Pedido recibido",
                "Pedido despachado",
                "Pedido entregado",
                "Cancelado"
            };

            if (!estadosValidos.Contains(nuevoEstado))
                return BadRequest("Estado inválido");

            venta.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            var mensaje = new MensajePedido
            {
                VentaId = venta.Id,
                ClienteNombre = venta.Cliente.Nombre,
                ClienteEmail = venta.Cliente.Email,
                Total = venta.Total,
                Estado = nuevoEstado,
                FechaEvento = DateTime.UtcNow
            };

            await _queueService.EnviarMensajeAsync(mensaje);

            return Ok(new
            {
                total = venta.Total
            });
        }
    }
}