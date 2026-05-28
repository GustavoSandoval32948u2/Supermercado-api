using System.ComponentModel.DataAnnotations.Schema;

namespace supermercado.API.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}