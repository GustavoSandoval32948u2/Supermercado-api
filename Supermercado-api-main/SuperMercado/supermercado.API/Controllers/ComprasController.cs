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
                Total = 0
            };

            _context.Compras.Add(compra);
            await _context.SaveChangesAsync();

            decimal total = 0;

            foreach (var item in dto.Productos)
            {
                var producto = await _context.Productos.FindAsync(item.ProductoId);

                if (producto == null)
                    return NotFound($"Producto {item.ProductoId} no existe");

                // 🔥 aumentar stock
                producto.Stock += item.Cantidad;

                var detalle = new DetalleCompra
                {
                    CompraId = compra.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                };

                _context.DetalleCompras.Add(detalle);

                total += item.Cantidad * item.PrecioUnitario;
            }

            compra.Total = total;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Compra registrada correctamente",
                compraId = compra.Id,
                total
            });
        }

        // 🔷 VER COMPRAS
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .ToListAsync();

            return Ok(compras);
        }

        // 🔷 VER COMPRA POR ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
                return NotFound("Compra no encontrada");

            return Ok(compra);
        }

        // 🔷 ACTUALIZAR COMPRA
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CompraDTO dto)
        {
            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
                return NotFound("Compra no encontrada");

            // 🔥 revertir stock anterior
            foreach (var detalleAnterior in compra.Detalles)
            {
                var productoAnterior = await _context.Productos
                    .FindAsync(detalleAnterior.ProductoId);

                if (productoAnterior != null)
                {
                    productoAnterior.Stock -= detalleAnterior.Cantidad;
                }
            }

            // 🔥 eliminar detalles viejos
            _context.DetalleCompras.RemoveRange(compra.Detalles);

            compra.ProveedorId = dto.ProveedorId;
            compra.Total = 0;

            decimal total = 0;

            foreach (var item in dto.Productos)
            {
                var producto = await _context.Productos
                    .FindAsync(item.ProductoId);

                if (producto == null)
                    return NotFound($"Producto {item.ProductoId} no existe");

                // 🔥 volver a sumar stock
                producto.Stock += item.Cantidad;

                var detalle = new DetalleCompra
                {
                    CompraId = compra.Id,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                };

                _context.DetalleCompras.Add(detalle);

                total += item.Cantidad * item.PrecioUnitario;
            }

            compra.Total = total;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Compra actualizada correctamente",
                compraId = compra.Id,
                total
            });
        }

        // 🔷 ELIMINAR COMPRA
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
                return NotFound("Compra no encontrada");

            // 🔥 revertir stock
            foreach (var detalle in compra.Detalles)
            {
                var producto = await _context.Productos
                    .FindAsync(detalle.ProductoId);

                if (producto != null)
                {
                    producto.Stock -= detalle.Cantidad;
                }
            }

            _context.DetalleCompras.RemoveRange(compra.Detalles);

            _context.Compras.Remove(compra);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Compra eliminada correctamente"
            });
        }
    }
}