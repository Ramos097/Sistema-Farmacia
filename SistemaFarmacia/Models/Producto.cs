using System.ComponentModel.DataAnnotations;

namespace SistemaFarmacia.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        public decimal PrecioCompra { get; set; }

        [Required]
        public decimal PrecioVenta { get; set; }

        public int Stock { get; set; }

        public bool Activo { get; set; } = true;

        public int ProveedorId { get; set; }

        public Proveedor? Proveedor { get; set; }

        public int CategoriaId { get; set; }

        public Categoria? Categoria { get; set; }
    }
}
