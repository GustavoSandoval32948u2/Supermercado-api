using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Controllers;
using supermercado.API.Data;
using supermercado.API.DTOs;
using supermercado.API.Models;
using Xunit;


namespace supermercado.API.Tests
{
    public class ApiControllerTests
    {
        [Fact]
        public async Task Productos_Get_DeberiaRetornarProductos()
        {
            using var db = CrearDbContext();

            db.Productos.Add(new Producto
            {
                Nombre = "Arroz Gallo Dorado 5 lb",
                Descripcion = "Producto basico",
                Precio = 10.00m,
                Stock = 50,
                Categoria = "Granos"
            });

            await db.SaveChangesAsync();

            var controller = new ProductosController(db);

            var resultado = await controller.GetProductos();

            var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;

            var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
            var doc = System.Text.Json.JsonDocument.Parse(json);

            var productos = doc.RootElement.GetProperty("data");

            productos.GetArrayLength().Should().Be(1);
        }

        [Fact]
        public async Task Productos_Create_DeberiaCrearProducto()
        {
            using var db = CrearDbContext();
            var controller = new ProductosController(db);

            var nuevo = new Producto
            {
                Nombre = "Leche Entera 1 litro",
                Descripcion = "Lacteo",
                Precio = 8.00m,
                Stock = 30,
                Categoria = "Lacteos"
            };

            var resultado = await controller.Create(nuevo);

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
        }

        [Fact]
        public async Task Ventas_CrearVenta_ConStock_DeberiaCalcularTotalYReducirStock()
        {
            using var db = CrearDbContext();

            db.Clientes.Add(new Cliente
            {
                Nombre = "Carlos Ruiz",
                Email = "carlos@mail.com",
                Telefono = "5555"
            });

            db.Productos.Add(new Producto
            {
                Nombre = "Cafe molido",
                Descripcion = "Cafe",
                Precio = 20.00m,
                Stock = 15,
                Categoria = "Cafe"
            });

            await db.SaveChangesAsync();

            var controller = new VentasController(db, null!);

            var dto = new VentaDTO
            {
                ClienteId = db.Clientes.First().Id,
                Productos = new List<DetalleVentaDTO>
                {
                    new() { ProductoId = db.Productos.First().Id, Cantidad = 2 }
                }
            };

            var resultado = await controller.CrearVenta(dto);

            var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;

            var json = System.Text.Json.JsonSerializer.Serialize(ok.Value);
            var doc = System.Text.Json.JsonDocument.Parse(json);

            var total = doc.RootElement.GetProperty("total").GetDecimal();

            total.Should().Be(40.00m);

            db.Productos.First().Stock.Should().Be(13);
        }

        [Fact]
        public async Task Ventas_SinStock_DeberiaFallar()
        {
            using var db = CrearDbContext();

            db.Clientes.Add(new Cliente
            {
                Nombre = "Ana",
                Email = "ana@mail.com",
                Telefono = "555"
            });

            db.Productos.Add(new Producto
            {
                Nombre = "Aceite",
                Precio = 18,
                Stock = 1,
                Categoria = "Aceites",
                Descripcion = "Aceite"
            });

            await db.SaveChangesAsync();

            var controller = new VentasController(db, null!);

            var dto = new VentaDTO
            {
                ClienteId = db.Clientes.First().Id,
                Productos = new List<DetalleVentaDTO>
                {
                    new() { ProductoId = db.Productos.First().Id, Cantidad = 5 }
                }
            };

            var resultado = await controller.CrearVenta(dto);

            resultado.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Carrito_AgregarProducto_DeberiaFuncionar()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            context.Productos.Add(new Producto
            {
                Nombre = "Arroz",
                Precio = 10,
                Stock = 20,
                Categoria = "Granos",
                Descripcion = "Arroz"
            });

            context.SaveChanges();

            var controller = new CarritoController(context, null!);

            var resultado = await controller.AgregarProducto(1, 2);

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
}