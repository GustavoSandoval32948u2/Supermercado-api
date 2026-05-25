using Microsoft.AspNetCore.Mvc;
using supermercado.API.Data;
using supermercado.API.Models;

namespace supermercado.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private static List<CarritoItem> carrito =
            new List<CarritoItem>();


        [HttpPost("agregar/{id}")]
        public IActionResult AgregarProducto(int id, int cantidad = 1)
        {
            var producto = DatosMock.Productos
                .FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                return NotFound("Producto no encontrado");
            }

            carrito.Add(new CarritoItem()
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Cantidad = cantidad,
                Precio = producto.Precio
            });

            return Ok("Producto agregado");
        }


        [HttpGet]
        public IActionResult VerCarrito()
        {
            return Ok(carrito);
        }


        [HttpPost("comprar")]
        public IActionResult Comprar()
        {
            decimal total = 0;

            foreach (var item in carrito)
            {
                total += item.Precio * item.Cantidad;
            }

            carrito.Clear();

            return Ok(new
            {
                mensaje = "Compra realizada",
                total = total
            });
        }
    }
}