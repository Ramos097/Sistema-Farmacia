using Microsoft.EntityFrameworkCore;
using SistemaFarmacia.Models;

namespace SistemaFarmacia.Data
{
    public class FarmaciaContext : DbContext
    {
        public FarmaciaContext(DbContextOptions<FarmaciaContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
    }
}