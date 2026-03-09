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
    }
}