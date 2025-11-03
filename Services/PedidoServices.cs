using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repaso3.DAL;
using Repaso3.Models;

namespace Repaso3.Services
{
    public class PedidoServices(IDbContextFactory<Contexto> DbFactory)
    {
        private enum Operacion 
        { 
            Suma = 1,
            Resta = 2
        }

        private async Task AfectarExistencia(ICollection<PedidoDetalle> detalle, Operacion tipoOperacion)
        {
            await using var contexto = await DbFactory.CreateDbContextAsync();
            foreach (var item in detalle)
            {
                var producto = await contexto.Productos
                    .SingleAsync(p => p.ProductoId == item.ProductoId);

                var cantidadPedido = item.Cantidad;

                if (tipoOperacion == Operacion.Suma)
                    producto.Existencia += cantidadPedido; 
                else
                    producto.Existencia -= cantidadPedido;

                await contexto.SaveChangesAsync();
            }
        }

        public async Task<bool> Existe(int id)
        {
            await using var contexto = await DbFactory.CreateDbContextAsync();
            return await contexto.Pedidos.AnyAsync(p => p.PedidoId == id);
        }

        public async Task<bool> Insertar(Pedido pedido)
        {
            await using var contexto = await DbFactory.CreateDbContextAsync();

            contexto.Add(pedido);

            await AfectarExistencia(pedido.Detalle, Operacion.Resta);

            return await contexto.SaveChangesAsync() > 0;
        }

        public async Task<bool> Modificar(Pedido pedido)
        {
            await using var contexto = await DbFactory.CreateDbContextAsync();
            var original = await contexto.Pedidos
                .Include(p => p.Detalle)
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.PedidoId == pedido.PedidoId);

            if (original == null) return false;

         
            await AfectarExistencia(original.Detalle, Operacion.Suma);

          
            contexto.PedidoDetalles.RemoveRange(original.Detalle);

           
            contexto.Update(pedido);

          
            await AfectarExistencia(pedido.Detalle, Operacion.Resta);

            return await contexto.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(int PedidoId)
        {
            await using var contexto = await DbFactory.CreateDbContextAsync();

            var entidad = await contexto.Pedidos
                .Include(p => p.Detalle)
                .FirstOrDefaultAsync(p => p.PedidoId == PedidoId);

            if (entidad is null) return false;

          
            await AfectarExistencia(entidad.Detalle, Operacion.Suma);

            contexto.PedidoDetalles.RemoveRange(entidad.Detalle);
            contexto.Pedidos.Remove(entidad);

            return await contexto.SaveChangesAsync() > 0;
        }

        public async Task<Pedido> Buscar(int pedidoId)
        {
            await using var contexto = await DbFactory.CreateDbContextAsync();
            return await contexto.Pedidos
                .Include(p => p.Detalle)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);
        }
        public async Task<bool> Guardar(Pedido pedido)
        {
            // Usa el PedidoId para verificar si existe
            if (!await Existe(pedido.PedidoId))
            {
                return await Insertar(pedido);
            }
            else
            {
                return await Modificar(pedido);
            }
        }

        public async Task<List<Pedido>> Listar(Expression<Func<Pedido, bool>> criterio)
        {
            using var ctx = await DbFactory.CreateDbContextAsync();
            return await ctx.Pedidos
                            .Include(p => p.Detalle)
                            .Where(criterio)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<List<Productos>> GetProductos()
        {
            using var ctx = await DbFactory.CreateDbContextAsync();
            return await ctx.Productos
                .AsNoTracking()
                .ToListAsync();
        }
    }

}

