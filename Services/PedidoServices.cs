using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repaso3.DAL;
using Repaso3.Models;

namespace Repaso3.Services;

public class PedidoServices(IDbContextFactory<Contexto> DbFactory)
{
    // Enum para definir si sumamos o restamos al inventario
    public enum TipoOperacion
    {
        Suma = 1,
        Resta = 2
    }

    private async Task AfectarExistencia(ICollection<PedidoDetalle> detalle, TipoOperacion tipoOperacion)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        foreach (var item in detalle)
        {
            var producto = await contexto.Productos
                .SingleOrDefaultAsync(p => p.ProductoId == item.ProductoId);

            if (producto == null) continue; // Si el producto no existe, saltar

            var cantidadPedido = item.Cantidad;

            if (tipoOperacion == TipoOperacion.Suma)
                producto.Existencia += cantidadPedido; // Devuelve al inventario
            else
                producto.Existencia -= cantidadPedido; // Saca del inventario

            // No llames a SaveChangesAsync() aquí dentro del bucle,
            // llámalo una sola vez en el método que origina el cambio (Insertar, Modificar, Eliminar)
        }
        // Guardamos los cambios de existencia al final del bucle
        await contexto.SaveChangesAsync();
    }
    public async Task<bool> Existe(int id)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Pedidos.AnyAsync(p => p.PedidoId == id);
    }

    public async Task<bool> Insertar(Pedido pedido)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        // Al insertar un pedido, RESTAMOS la cantidad del inventario
        await AfectarExistencia(pedido.Detalle, TipoOperacion.Resta);

        // Agregamos el pedido (que ya trae el Total calculado desde la página razor)
        contexto.Add(pedido);

        return await contexto.SaveChangesAsync() > 0;
    }

    // --- ESTE ES EL MÉTODO CORREGIDO ---
    public async Task<bool> Modificar(Pedido pedido)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        // 1. Buscamos el pedido original, PERO CON TRACKING (SIN AsNoTracking)
        var original = await contexto.Pedidos
            .Include(p => p.Detalle)
            .SingleOrDefaultAsync(p => p.PedidoId == pedido.PedidoId);

        if (original == null) return false;

        // 2. Devolvemos las cantidades del pedido original al inventario (SUMA)
        await AfectarExistencia(original.Detalle, TipoOperacion.Suma);

        // 3. Borramos los detalles antiguos de la base de datos
        contexto.PedidoDetalles.RemoveRange(original.Detalle);
        await contexto.SaveChangesAsync(); // Aplicamos la eliminación del detalle

        // 4. Actualizamos manualmente las propiedades del pedido original
        //    (Aquí es donde nos aseguramos de que el TOTAL se actualice)
        original.Fecha = pedido.Fecha;
        original.ClienteNombre = pedido.ClienteNombre;
        original.Total = pedido.Total; // <-- ¡LA LÍNEA CLAVE!
        original.Detalle = pedido.Detalle; // Asignamos la nueva lista de detalles

        // 5. Restamos las nuevas cantidades del inventario (RESTA)
        // (Este era el otro bug, antes decía Suma)
        await AfectarExistencia(original.Detalle, TipoOperacion.Resta);

        // 6. Actualizamos la entidad principal
        contexto.Update(original);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Eliminar(int PedidoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        var entidad = await contexto.Pedidos
            .Include(p => p.Detalle)
            .FirstOrDefaultAsync(p => p.PedidoId == PedidoId);

        if (entidad is null) return false;

        // Al eliminar un pedido, devolvemos las cantidades al inventario (SUMA)
        await AfectarExistencia(entidad.Detalle, TipoOperacion.Suma);

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

    // Método para obtener la lista de productos
    public async Task<List<Productos>> GetProductos()
    {
        using var ctx = await DbFactory.CreateDbContextAsync();
        return await ctx.Productos
            .AsNoTracking()
            .ToListAsync();
    }
}

