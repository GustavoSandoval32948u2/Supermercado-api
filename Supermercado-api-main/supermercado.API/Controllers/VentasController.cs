using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.DTOs;
using supermercado.API.Models;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        // 🔷 CREAR VENTA
        [HttpPost]
        public async Task<IActionResult> CrearVenta(VentaDTO dto)
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return NotFound("Cliente no existe");

            var venta = new Venta
            {
                ClienteId = dto.ClienteId,
                Fecha = DateTime.Now,
                Estado = "Pendiente",
                Detalles = new List<DetalleVenta>(),
                Total = 0
            };

            foreach (var item in dto.Productos)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);

                if (producto == null)
                    return NotFound($"Producto {item.ProductoId} no existe");

                if (producto.Stock < item.Cantidad)
                    return BadRequest($"Stock insuficiente para {producto.Nombre}");

                producto.Stock -= item.Cantidad;

                var detalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = producto.Precio
                };

                venta.Total += detalle.SubTotal;
                venta.Detalles.Add(detalle);
            }

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return Ok(venta);
        }

        // 🔷 VER VENTAS
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .Include(v => v.Cliente)
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
                return NotFound("Venta no encontrada");

            venta.Estado = nuevoEstado;

            await _context.SaveChangesAsync();

            return Ok(venta);
        }
    }
}