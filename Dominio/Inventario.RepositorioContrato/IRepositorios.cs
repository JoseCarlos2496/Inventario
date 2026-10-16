using Inventario.Agregados;

namespace Inventario.RepositorioContrato
{
    public interface IRepositorioExistencia
    {
        /// <summary>Existencias del producto en todas sus ubicaciones (en seguimiento, para modificarlas).</summary>
        Task<List<Existencia>> ListarPorProductoAsync(Guid productoId, CancellationToken cancellationToken);

        Task<List<Existencia>> ListarPorProductosAsync(IReadOnlyCollection<Guid> productoIds, CancellationToken cancellationToken);

        Task<List<Existencia>> ListarAsync(CancellationToken cancellationToken);

        void Guardar(Existencia existencia);
    }

    public interface IRepositorioAsignacion
    {
        Task<AsignacionOrden?> BuscarPorOrdenAsync(Guid ordenCompraId, CancellationToken cancellationToken);

        void Guardar(AsignacionOrden asignacion);
    }

    public interface IRepositorioMovimiento
    {
        Task<List<MovimientoStock>> ListarPorProductoAsync(Guid productoId, CancellationToken cancellationToken);

        void Guardar(MovimientoStock movimiento);
    }

    public interface IRepositorioInventario
    {
        Task<InventarioProducto?> BuscarPorProductoAsync(Guid productoId, CancellationToken cancellationToken);

        Task<List<InventarioProducto>> ListarPorProductosAsync(IReadOnlyCollection<Guid> productoIds, CancellationToken cancellationToken);

        void Guardar(InventarioProducto inventario);
    }

    public interface IRepositorioProducto
    {
        Task<Producto?> BuscarAsync(Guid productoId, CancellationToken cancellationToken);

        Task<Producto?> BuscarPorSkuAsync(string sku, CancellationToken cancellationToken);

        /// <summary>Productos cuyo id o sku (ya normalizado) esté en las listas.</summary>
        Task<List<Producto>> ListarPorIdsOSkusAsync(IReadOnlyCollection<Guid> productoIds, IReadOnlyCollection<string> skus, CancellationToken cancellationToken);

        Task<List<Producto>> ListarAsync(CancellationToken cancellationToken);

        void Guardar(Producto producto);
    }

    public interface IRepositorioUbicacion
    {
        Task<Ubicacion?> BuscarPorCodigoAsync(string codigo, CancellationToken cancellationToken);

        Task<List<Ubicacion>> ListarAsync(CancellationToken cancellationToken);

        Task<bool> ExisteAlgunaAsync(CancellationToken cancellationToken);

        void Guardar(Ubicacion ubicacion);
    }
}
