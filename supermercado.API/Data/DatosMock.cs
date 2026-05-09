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
                Precio = 5,
                Stock = 40
            },

            new Producto
            {
                Id = 4,
                Nombre = "Jabon",
                Precio = 12,
                Stock = 25
            },

            new Producto
            {
                Id = 5,
                Nombre = "Servilletas",
                Precio = 6,
                Stock = 35
            },

            new Producto
            {
                Id = 6,
                Nombre = "Cafe",
                Precio = 20,
                Stock = 15
            },

            new Producto
            {
                Id = 7,
                Nombre = "Aceite",
                Precio = 18,
                Stock = 20
            },

            new Producto
            {
                Id = 8,
                Nombre = "Agua",
                Precio = 4,
                Stock = 60
            },

            new Producto
            {
                Id = 9,
                Nombre = "Suavitel",
                Precio = 22,
                Stock = 10
            }
        };
    }
}