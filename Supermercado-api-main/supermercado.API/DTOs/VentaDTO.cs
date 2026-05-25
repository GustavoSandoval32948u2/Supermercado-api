namespace supermercado.API.DTOs
{
    public class VentaDTO
    {
        public int ClienteId { get; set; }

        public List<DetalleVentaDTO> Productos { get; set; }
    }

    public class DetalleVentaDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}
