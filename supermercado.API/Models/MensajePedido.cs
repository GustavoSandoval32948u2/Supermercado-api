namespace supermercado.API.Models
{
    // este es el objeto que se mete en la cola de azure
    // tiene todo lo que necesita el consumidor para mandar el correo al cliente
    public class MensajePedido
    {
        public int VentaId { get; set; }

        // los tres estados del ciclo: Recibido, Despachado, Entregado
        public string Estado { get; set; } = string.Empty;

        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;

        public decimal Total { get; set; }

        // cuando se genero el mensaje, no cuando se hizo la venta
        public DateTime FechaEvento { get; set; } = DateTime.UtcNow;

        public string? DireccionEntrega { get; set; }
        public string? NumeroSeguimiento { get; set; }
    }
}
