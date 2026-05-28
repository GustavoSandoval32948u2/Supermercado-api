namespace supermercado.API.DTOs
{
    public class CompraDTO
    {
        public int ProveedorId { get; set; }

        public List<DetalleCompraDTO> Productos { get; set; }
    }

    public class DetalleCompraDTO
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
