using supermercado.API.Models;

namespace supermercado.API.Data
{
    public static class DatosMock
    {
        public static List<Producto> Productos = new List<Producto>()
        {
            new Producto
            {
                Id = 1,
                Nombre = "Arroz",
                Precio = 10,
                Stock = 50
            },

            new Producto
            {
                Id = 2,
                Nombre = "Leche",
                Precio = 8,
                Stock = 30
            },

            new Producto
            {
                Id = 3,
                Nombre = "CocaCola",
                Precio = 15,
                Stock = 20
            },

            new Producto
            {
                Id = 4,
                Nombre = "Galletas",
                Precio = 12,
                Stock = 40
            },

            new Producto
            {
                Id = 5,
                Nombre = "Jabon",
                Precio = 18,
                Stock = 25
            },

            new Producto
            {
                Id = 6,
                Nombre = "Servilletas",
                Precio = 7,
                Stock = 60
            },

            new Producto
            {
                Id = 7,
                Nombre = "Cafe",
                Precio = 22,
                Stock = 15
            },

            new Producto
            {
                Id = 8,
                Nombre = "Aceite",
                Precio = 35,
                Stock = 18
            },

            new Producto
            {
                Id = 9,
                Nombre = "Agua",
                Precio = 5,
                Stock = 100
            },

            new Producto
            {
                Id = 10,
                Nombre = "Suavitel",
                Precio = 28,
                Stock = 12
            }
        };
    }
}