namespace SistemaFarmacia.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
