using Inventario.Agregados;
using Inventario.ObjetosValor;
using Inventario.RepositorioContrato;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infraestructura.Repositorio
{
    public class RepositorioExistencia : RepositorioBase<Existencia>, IRepositorioExistencia
    {
        public RepositorioExistencia(DbContext dbContext) : base(dbContext) { }

        public Task<List<Existencia>> ListarPorProductoAsync(Guid productoId, CancellationToken cancellationToken)
        {
            return _dbSet.Where(e => e.ProductoId == productoId).ToListAsync(cancellationToken);
        }

        public Task<List<Existencia>> ListarPorProductosAsync(IReadOnlyCollection<Guid> productoIds, CancellationToken cancellationToken)
        {
            return _dbSet.Where(e => productoIds.Contains(e.ProductoId)).ToListAsync(cancellationToken);
        }

        public Task<List<Existencia>> ListarAsync(CancellationToken cancellationToken)
        {
            return _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }
    }

    public class RepositorioAsignacion : RepositorioBase<AsignacionOrden>, IRepositorioAsignacion
    {
        public RepositorioAsignacion(DbContext dbContext) : base(dbContext) { }

        public Task<AsignacionOrden?> BuscarPorOrdenAsync(Guid ordenCompraId, CancellationToken cancellationToken)
        {
            return _dbSet
                .Include(a => a.Detalles)
                .Include(a => a.Errores)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.OrdenCompraId == ordenCompraId, cancellationToken);
        }
    }

    public class RepositorioMovimiento : RepositorioBase<MovimientoStock>, IRepositorioMovimiento
    {
        public RepositorioMovimiento(DbContext dbContext) : base(dbContext) { }

        public Task<List<MovimientoStock>> ListarPorProductoAsync(Guid productoId, CancellationToken cancellationToken)
        {
            return _dbSet.AsNoTracking()
                .Where(m => m.ProductoId == productoId)
                .OrderBy(m => m.Fecha)
                .ToListAsync(cancellationToken);
        }
    }

    public class RepositorioInventario : RepositorioBase<InventarioProducto>, IRepositorioInventario
    {
        public RepositorioInventario(DbContext dbContext) : base(dbContext) { }

        public Task<InventarioProducto?> BuscarPorProductoAsync(Guid productoId, CancellationToken cancellationToken)
        {
            return _dbSet.FirstOrDefaultAsync(i => i.ProductoId == productoId, cancellationToken);
        }

        public Task<List<InventarioProducto>> ListarPorProductosAsync(IReadOnlyCollection<Guid> productoIds, CancellationToken cancellationToken)
        {
            return _dbSet.AsNoTracking().Where(i => productoIds.Contains(i.ProductoId)).ToListAsync(cancellationToken);
        }
    }

    public class RepositorioProducto : RepositorioBase<Producto>, IRepositorioProducto
    {
        public RepositorioProducto(DbContext dbContext) : base(dbContext) { }

        public Task<Producto?> BuscarAsync(Guid productoId, CancellationToken cancellationToken)
        {
            return _dbSet.FirstOrDefaultAsync(p => p.Id == productoId, cancellationToken);
        }

        public Task<Producto?> BuscarPorSkuAsync(string sku, CancellationToken cancellationToken)
        {
            var valor = Sku.Crear(sku);
            return _dbSet.FirstOrDefaultAsync(p => p.Sku == valor, cancellationToken);
        }

        public Task<List<Producto>> ListarPorIdsOSkusAsync(IReadOnlyCollection<Guid> productoIds, IReadOnlyCollection<string> skus, CancellationToken cancellationToken)
        {
            // Un SKU con formato inválido simplemente no coincide con ningún producto (la orden lo reporta como ORD-101).
            var valores = skus.Where(s => s.Length <= Sku.LongitudMaxima).Select(Sku.Crear).ToList();
            return _dbSet.Where(p => productoIds.Contains(p.Id) || valores.Contains(p.Sku)).ToListAsync(cancellationToken);
        }

        public async Task<List<Producto>> ListarAsync(CancellationToken cancellationToken)
        {
            // Sku es un objeto de valor: se ordena por su texto ya en memoria (no todos los proveedores saben compararlo).
            var productos = await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
            return productos.OrderBy(p => p.Sku.Valor).ToList();
        }
    }

    public class RepositorioUbicacion : RepositorioBase<Ubicacion>, IRepositorioUbicacion
    {
        public RepositorioUbicacion(DbContext dbContext) : base(dbContext) { }

        public Task<Ubicacion?> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken)
        {
            return _dbSet.FirstOrDefaultAsync(u => u.Codigo == codigo, cancellationToken);
        }

        public Task<List<Ubicacion>> ListarAsync(CancellationToken cancellationToken)
        {
            return _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        public Task<bool> ExisteAlgunaAsync(CancellationToken cancellationToken)
        {
            return _dbSet.AnyAsync(cancellationToken);
        }
    }
}
