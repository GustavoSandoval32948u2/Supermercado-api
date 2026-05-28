using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Controllers;
using supermercado.API.Data;
using supermercado.API.DTOs;
using supermercado.API.Models;
using Xunit;

namespace supermercado.API.Tests;

public class ApiControllerTests
{
    [Fact]
    public async Task Productos_Get_DeberiaRetornarProductos()
    {
        using var db = CrearDbContext();
        db.Productos.Add(new Producto
        {
            Id = 1,
            Nombre = "Arroz Gallo Dorado 5 lb",
            Descripcion = "Producto basico de supermercado",
            Precio = 10.00m,
            Stock = 50
        });
        await db.SaveChangesAsync();

        var controller = new ProductosController(db);

        var resultado = await controller.GetAll();

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var productos = ok.Value.Should().BeAssignableTo<IEnumerable<Producto>>().Subject.ToList();
        productos.Should().ContainSingle();
        productos[0].Nombre.Should().Be("Arroz Gallo Dorado 5 lb");
    }

    [Fact]
    public async Task Productos_Create_DeberiaCrearProductoConDatosCoherentes()
    {
        using var db = CrearDbContext();
        var controller = new ProductosController(db);

        var nuevoProducto = new Producto
        {
            Nombre = "Leche Entera 1 litro",
            Descripcion = "Lacteo refrigerado",
            Precio = 8.00m,
            Stock = 30
        };

        var resultado = await controller.Create(nuevoProducto);

        resultado.Should().BeOfType<CreatedAtActionResult>();
        db.Productos.Should().ContainSingle(p => p.Nombre == "Leche Entera 1 litro");
    }

    [Fact]
    public async Task Clientes_Create_DeberiaRegistrarCliente()
    {
        using var db = CrearDbContext();
        var controller = new ClientesController(db);

        var cliente = new Cliente
        {
            Nombre = "Maria Lopez",
            Email = "maria.lopez@gmail.com",
            Telefono = "5555-1111"
        };

        var resultado = await controller.Create(cliente);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(cliente);
        db.Clientes.Should().ContainSingle(c => c.Email == "maria.lopez@gmail.com");
    }

    [Fact]
    public async Task Ventas_CrearVenta_ConStockSuficiente_DeberiaDisminuirStockYCalcularTotal()
    {
        using var db = CrearDbContext();
        db.Clientes.Add(new Cliente { Id = 1, Nombre = "Carlos Ruiz", Email = "carlos.ruiz@hotmail.com", Telefono = "5555-2222" });
        db.Productos.Add(new Producto { Id = 1, Nombre = "Cafe molido 400 g", Descripcion = "Cafe", Precio = 20.00m, Stock = 15 });
        await db.SaveChangesAsync();

        var controller = new VentasController(db);
        var dto = new VentaDTO
        {
            ClienteId = 1,
            Productos = new List<DetalleVentaDTO>
            {
                new() { ProductoId = 1, Cantidad = 2 }
            }
        };

        var resultado = await controller.CrearVenta(dto);

        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var venta = ok.Value.Should().BeOfType<Venta>().Subject;
        venta.Total.Should().Be(40.00m);
        db.Productos.Single(p => p.Id == 1).Stock.Should().Be(13);
    }

    [Fact]
    public async Task Ventas_CrearVenta_SinStock_DeberiaRetornarBadRequest()
    {
        using var db = CrearDbContext();
        db.Clientes.Add(new Cliente { Id = 1, Nombre = "Ana Martinez", Email = "ana.mtz@gmail.com", Telefono = "5555-3333" });
        db.Productos.Add(new Producto { Id = 1, Nombre = "Aceite vegetal 900 ml", Descripcion = "Aceite", Precio = 18.00m, Stock = 1 });
        await db.SaveChangesAsync();

        var controller = new VentasController(db);
        var dto = new VentaDTO
        {
            ClienteId = 1,
            Productos = new List<DetalleVentaDTO>
            {
                new() { ProductoId = 1, Cantidad = 5 }
            }
        };

        var resultado = await controller.CrearVenta(dto);

        var badRequest = resultado.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value!.ToString().Should().Contain("Stock insuficiente");
    }

    [Fact]
    public void Carrito_AgregarProducto_DeberiaAceptarProductoMock()
    {
        var controller = new CarritoController();

        var resultado = controller.AgregarProducto(1, 2);

        resultado.Should().BeOfType<OkObjectResult>();
    }

    private static AppDbContext CrearDbContext()
    {
        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(opciones);
    }
}
