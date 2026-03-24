namespace SistemaFarmacia.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        // Relación
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
