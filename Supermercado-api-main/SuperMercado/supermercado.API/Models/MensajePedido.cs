using supermercado.API.DTOs;

public class MensajePedido
{
    public int VentaId { get; set; }

    public int ClienteId { get; set; }

    public List<DetalleVentaDTO> Productos { get; set; } = new();

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