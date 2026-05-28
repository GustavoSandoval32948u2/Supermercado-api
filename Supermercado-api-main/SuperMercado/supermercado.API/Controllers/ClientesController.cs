using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.Models;


namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .ToListAsync();

            return Ok(clientes);
        }

        // GET: api/clientes/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound("Cliente no encontrado");

            return Ok(cliente);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (cliente == null)
                return BadRequest("Cliente inválido");

            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                return BadRequest("Nombre requerido");

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(cliente);
        }

        // PUT: api/clientes/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Cliente cliente)
        {
            var existente = await _context.Clientes.FindAsync(id);

            if (existente == null)
                return NotFound("Cliente no encontrado");

            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                return BadRequest("Nombre requerido");

            existente.Nombre = cliente.Nombre;
            existente.Email = cliente.Email;
            existente.Telefono = cliente.Telefono;

            await _context.SaveChangesAsync();

            return Ok(existente);
        }

        // DELETE: api/clientes/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound("Cliente no encontrado");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cliente eliminado correctamente" });
        }
    }
}