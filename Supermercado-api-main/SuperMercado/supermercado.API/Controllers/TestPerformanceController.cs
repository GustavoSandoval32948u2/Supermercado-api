using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Data;
using supermercado.API.Models;
using supermercado.API.Services;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/test-performance")]
    public class TestPerformanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly QueueService _queueService;

        public TestPerformanceController(AppDbContext context, QueueService queueService)
        {
            _context = context;
            _queueService = queueService;
        }

        // 🔵 SIMULACIÓN SINCRONA
        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            var productos = await _context.Productos.ToListAsync();

            await Task.Delay(200);

            return Ok(new
            {
                mode = "sync",
                count = productos.Count
            });
        }

        // 🟢 SIMULACIÓN ASÍNCRONA
        [HttpPost("async")]
        public async Task<IActionResult> Async()
        {
            var productos = await _context.Productos.ToListAsync();

            var mensaje = new MensajePedido
            {
                VentaId = 0,
                ClienteNombre = "TEST",
                ClienteEmail = "test@test.com",
                Total = productos.Count,
                Estado = "ASYNC_TEST",
                FechaEvento = DateTime.UtcNow
            };

            await _queueService.EnviarMensajeAsync(mensaje);

            return Ok(new
            {
                mode = "async",
                count = productos.Count
            });
        }
    }
}