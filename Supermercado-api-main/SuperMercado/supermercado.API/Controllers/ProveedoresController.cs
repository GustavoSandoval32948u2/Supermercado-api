using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.Models;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // 🔷 GET: api/proveedores
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var proveedores = await _context.Proveedores
                .AsNoTracking()
                .ToListAsync();

            return Ok(proveedores);
        }

        // 🔷 GET: api/proveedores/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var proveedor = await _context.Proveedores
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
                return NotFound(new
                {
                    mensaje = "Proveedor no encontrado"
                });

            return Ok(proveedor);
        }

        // 🔷 POST: api/proveedores
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Proveedor proveedor)
        {
            if (proveedor == null)
                return BadRequest("Proveedor inválido");

            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
                return BadRequest("Nombre requerido");

            _context.Proveedores.Add(proveedor);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = proveedor.Id },
                proveedor
            );
        }

        // 🔷 PUT: api/proveedores/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Proveedor proveedor)
        {
            var existente = await _context.Proveedores.FindAsync(id);

            if (existente == null)
                return NotFound(new
                {
                    mensaje = "Proveedor no encontrado"
                });

            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
                return BadRequest("Nombre requerido");

            existente.Nombre = proveedor.Nombre;
            existente.Contacto = proveedor.Contacto;
            existente.Telefono = proveedor.Telefono;

            await _context.SaveChangesAsync();

            return Ok(existente);
        }

        // 🔷 DELETE: api/proveedores/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);

            if (proveedor == null)
                return NotFound(new
                {
                    mensaje = "Proveedor no encontrado"
                });

            _context.Proveedores.Remove(proveedor);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Proveedor eliminado correctamente"
            });
        }
    }
}