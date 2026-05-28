using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        // 🔷 VENTAS POR FECHA
        [HttpGet("ventas-por-fecha")]
        public async Task<IActionResult> VentasPorFecha(DateTime inicio, DateTime fin)
        {
            if (inicio > fin)
                return BadRequest("La fecha inicio no puede ser mayor que la fecha fin");

            var ventas = await _context.Ventas
                .Where(v => v.Fecha >= inicio && v.Fecha <= fin)
                .Include(v => v.Cliente)
                .AsNoTracking()
                .ToListAsync();

            var total = ventas.Sum(v => v.Total);

            return Ok(new
            {
                FechaInicio = inicio,
                FechaFin = fin,
                CantidadVentas = ventas.Count,
                TotalVendido = total,
                Ventas = ventas
            });
        }

        // 🔷 VENTAS POR CLIENTE
        [HttpGet("ventas-por-cliente/{clienteId}")]
        public async Task<IActionResult> VentasPorCliente(int clienteId)
        {
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == clienteId);

            if (!clienteExiste)
                return NotFound(new
                {
                    mensaje = "Cliente no encontrado"
                });

            var ventas = await _context.Ventas
                .Where(v => v.ClienteId == clienteId)
                .Include(v => v.Cliente)
                .AsNoTracking()
                .ToListAsync();

            var total = ventas.Sum(v => v.Total);

            return Ok(new
            {
                ClienteId = clienteId,
                CantidadVentas = ventas.Count,
                TotalComprado = total,
                Ventas = ventas
            });
        }

        // 🔷 COMPRAS POR PROVEEDOR
        [HttpGet("compras-por-proveedor/{proveedorId}")]
        public async Task<IActionResult> ComprasPorProveedor(int proveedorId)
        {
            var proveedorExiste = await _context.Proveedores.AnyAsync(p => p.Id == proveedorId);

            if (!proveedorExiste)
                return NotFound(new
                {
                    mensaje = "Proveedor no encontrado"
                });

            var compras = await _context.Compras
                .Where(c => c.ProveedorId == proveedorId)
                .Include(c => c.Proveedor)
                .AsNoTracking()
                .ToListAsync();

            var total = compras.Sum(c => c.Total);

            return Ok(new
            {
                ProveedorId = proveedorId,
                CantidadCompras = compras.Count,
                TotalComprado = total,
                Compras = compras
            });
        }

        // 🔷 PRODUCTOS MÁS VENDIDOS
        [HttpGet("productos-mas-vendidos")]
        public async Task<IActionResult> ProductosMasVendidos()
        {
            var productos = await _context.DetalleVentas
                .Include(d => d.Producto)
                .AsNoTracking()
                .GroupBy(d => d.Producto.Nombre)
                .Select(g => new
                {
                    Producto = g.Key,
                    CantidadVendida = g.Sum(x => x.Cantidad),
                    TotalVentas = g.Sum(x => x.Cantidad * x.PrecioUnitario)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .ToListAsync();

            return Ok(productos);
        }
    }
}