using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.DTOs;
using supermercado.API.Models;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        // 🔷 REGISTRAR COMPRA
        [HttpPost]
        public async Task<IActionResult> CrearCompra(CompraDTO dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);

            if (proveedor == null)
                return NotFound("Proveedor no existe");

            var compra = new Compra
            {
                ProveedorId = dto.ProveedorId,
                Fecha = DateTime.Now,
                Total = 0,
                Detalles = new List<DetalleCompra>()
            };

            foreach (var item in dto.Productos)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);

                if (producto == null)
                    return NotFound($"Producto {item.ProductoId} no existe");

                // 🔥 AUMENTAR STOCK
                producto.Stock += item.Cantidad;

                var detalle = new DetalleCompra
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                };

                compra.Total += detalle.SubTotal;
                compra.Detalles.Add(detalle);
            }

            _context.Compras.Add(compra);

            await _context.SaveChangesAsync();

            return Ok(compra);
        }

        // 🔷 VER COMPRAS
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .ToListAsync();

            return Ok(compras);
        }
    }
}