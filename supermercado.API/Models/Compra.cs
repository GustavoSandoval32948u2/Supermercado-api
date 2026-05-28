namespace supermercado.API.Models
{
    public class Compra
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; }

        public decimal Total { get; set; }

        public List<DetalleCompra> Detalles { get; set; } = new();
    }
}
