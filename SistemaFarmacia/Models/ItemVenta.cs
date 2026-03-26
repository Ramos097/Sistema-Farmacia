namespace SistemaFarmacia.Models
{
    public class ItemVenta
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Subtotal => Precio * Cantidad;
    }
}
