using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.Models;



namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // 🔷 GET: api/productos
        [HttpGet]
        public async Task<IActionResult> GetProductos(
            int page = 1,
            int pageSize = 10,
            string? categoria = null)
        {
            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria == categoria);

            var total = await query.CountAsync();

            var productos = await query
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total = total,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize),
                data = productos
            });
        }

        // 🔷 GET: api/productos/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(producto);
        }

        // 🔷 POST: api/productos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Producto producto)
        {
            if (producto == null)
                return BadRequest("Producto inválido");

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                return BadRequest("Nombre requerido");

            if (producto.Precio <= 0)
                return BadRequest("Precio debe ser mayor a 0");

            if (producto.Stock < 0)
                return BadRequest("Stock inválido");

            _context.Productos.Add(producto);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = producto.Id },
                producto
            );
        }

        // 🔷 PUT: api/productos/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
        {
            var existente = await _context.Productos.FindAsync(id);

            if (existente == null)
                return NotFound(new { message = "Producto no encontrado" });

            if (string.IsNullOrWhiteSpace(producto.Nombre))
                return BadRequest("Nombre requerido");

            if (producto.Precio <= 0)
                return BadRequest("Precio debe ser mayor a 0");

            if (producto.Stock < 0)
                return BadRequest("Stock inválido");

            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.Precio = producto.Precio;
            existente.Stock = producto.Stock;
            existente.Categoria = producto.Categoria;

            await _context.SaveChangesAsync();

            return Ok(existente);
        }

        // 🔷 DELETE: api/productos/1    
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            _context.Productos.Remove(producto);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Producto eliminado correctamente"
            });
        }
    }
}