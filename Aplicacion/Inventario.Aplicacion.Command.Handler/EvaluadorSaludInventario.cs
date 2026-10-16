using Inventario.Agregados;
using Inventario.RepositorioContrato;

namespace Inventario.Aplicacion.Command.Handler
{
    /// <summary>
    /// Tras cada cambio de stock (con el total del producto en todas sus ubicaciones) revisa la política de stock mínimo del producto (la crea con mínimo 0 si no existe).
    /// </summary>
    public sealed class EvaluadorSaludInventario
    {
        private readonly IRepositorioInventario _repositorioInventario;

        public EvaluadorSaludInventario(IRepositorioInventario repositorioInventario)
        {
            _repositorioInventario = repositorioInventario;
        }

        public async Task<InventarioProducto> EvaluarAsync(Guid productoId, int cantidadTotal, CancellationToken cancellationToken)
        {
            var inventario = await _repositorioInventario.BuscarPorProductoAsync(productoId, cancellationToken);
            if (inventario is null)
            {
                inventario = InventarioProducto.Crear(productoId);
                _repositorioInventario.Guardar(inventario);
            }

            inventario.EvaluarSalud(cantidadTotal);
            return inventario;
        }
    }
}
