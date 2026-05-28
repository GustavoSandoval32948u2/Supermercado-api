using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using supermercado.API.Models;


using Microsoft.EntityFrameworkCore;
using supermercado.API.Models;

namespace supermercado.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetalleCompras { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔷 PRODUCTOS
            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            // 🔷 VENTAS
            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasPrecision(18, 2);

            // 🔷 DETALLE VENTAS
            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            // 🔷 COMPRAS
            modelBuilder.Entity<Compra>()
                .Property(c => c.Total)
                .HasPrecision(18, 2);

            // 🔷 DETALLE COMPRAS
            modelBuilder.Entity<DetalleCompra>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}