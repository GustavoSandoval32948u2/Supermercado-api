namespace supermercado.API.Models
{
    public class CarritoItem
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; }

        public int Cantidad { get; set; }

        public decimal Precio { get; set; }
    }
}