using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;

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
        public async Task<IActionResult> GetAll()
        {
            var productos = await _context.Productos.ToListAsync();
            return Ok(productos);
        }

        // 🔷 GET: api/productos/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(producto);
        }

        // 🔷 POST: api/productos
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Producto producto)
        {
            if (producto == null)
                return BadRequest();

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
        }

        // 🔷 PUT: api/productos/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
        {
            var existente = await _context.Productos.FindAsync(id);

            if (existente == null)
                return NotFound(new { message = "Producto no encontrado" });

            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.Precio = producto.Precio;
            existente.Stock = producto.Stock;

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

            return Ok(new { message = "Producto eliminado correctamente" });
        }
    }
}