using Microsoft.EntityFrameworkCore;
using Repaso3.Models;

namespace Repaso3.DAL;

public class Contexto : DbContext
{
    public Contexto(DbContextOptions<Contexto> options) : base(options) { }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
    public DbSet<Productos> Productos { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Productos>().HasData(
            new List<Productos>
            {
            new()
            {
                ProductoId = 1,
                Descripcion = "Habichuelas",
                Existencia = 150,
                Precio = 70
            },
            new()
            {
                ProductoId = 2,
                Descripcion = "Arroz",
                Existencia = 100,
                Precio = 50
            },
            new()
            {
                ProductoId = 3,
                Descripcion = "Pollo",
                Existencia = 50,
                Precio = 80
            }
            }
            );
    }
}