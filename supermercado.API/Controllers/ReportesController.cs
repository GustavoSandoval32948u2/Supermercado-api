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

        // 🔥 VENTAS POR FECHA
        [HttpGet("ventas-por-fecha")]
        public async Task<IActionResult> VentasPorFecha(DateTime inicio, DateTime fin)
        {
            var ventas = await _context.Ventas
                .Where(v => v.Fecha >= inicio && v.Fecha <= fin)
                .Include(v => v.Cliente)
                .ToListAsync();

            var total = ventas.Sum(v => v.Total);

            return Ok(new
            {
                CantidadVentas = ventas.Count,
                TotalVendido = total,
                Ventas = ventas
            });
        }

        // 🔥 VENTAS POR CLIENTE
        [HttpGet("ventas-por-cliente/{clienteId}")]
        public async Task<IActionResult> VentasPorCliente(int clienteId)
        {
            var ventas = await _context.Ventas
                .Where(v => v.ClienteId == clienteId)
                .Include(v => v.Cliente)
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

        // 🔥 COMPRAS POR PROVEEDOR
        [HttpGet("compras-por-proveedor/{proveedorId}")]
        public async Task<IActionResult> ComprasPorProveedor(int proveedorId)
        {
            var compras = await _context.Compras
                .Where(c => c.ProveedorId == proveedorId)
                .Include(c => c.Proveedor)
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

        // 🔥 PRODUCTOS MÁS VENDIDOS
        [HttpGet("productos-mas-vendidos")]
        public async Task<IActionResult> ProductosMasVendidos()
        {
            var productos = await _context.DetallesVenta
                .Select(d => new
                {
                    NombreProducto = d.Producto.Nombre,
                    Cantidad = d.Cantidad
                })
                .GroupBy(x => x.NombreProducto)
                .Select(g => new
                {
                    Producto = g.Key,
                    CantidadVendida = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .ToListAsync();

            return Ok(productos);
        }
    }
}