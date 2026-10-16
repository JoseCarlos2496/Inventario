using Inventario.Dominio.Comun;
using Inventario.Dominio.Eventos;

namespace Inventario.Agregados
{
    /// <summary>
    /// Agregado "Inventario": salud del inventario de un producto (sumando todas sus ubicaciones): política de stock mínimo y alerta cuando se cruza.
    /// </summary>
    public class InventarioProducto : AgregadoRoot
    {
        public Guid ProductoId { get; private set; }

        public int StockMinimo { get; private set; }

        public bool BajoMinimo { get; private set; }

        private InventarioProducto() { }

        private InventarioProducto(Guid productoId, int stockMinimo)
        {
            ProductoId = productoId;
            DefinirStockMinimo(stockMinimo);
        }

        public static InventarioProducto Crear(Guid productoId, int stockMinimo = 0)
        {
            return new InventarioProducto(productoId, stockMinimo);
        }

        public void DefinirStockMinimo(int stockMinimo)
        {
            if (stockMinimo < 0)
            {
                throw new ExcepcionValidacion(CatalogoErrores.StockMinimoInvalido);
            }
            StockMinimo = stockMinimo;
            MarcarModificado();
        }

        /// <summary>
        /// Avisa solo cuando el producto pasa de sano a bajo mínimo, no en cada movimiento.
        /// </summary>
        public void EvaluarSalud(int cantidadDisponible)
        {
            var bajoMinimo = cantidadDisponible < StockMinimo;
            if (bajoMinimo && !BajoMinimo)
            {
                AddDomainEvent(new StockBajoMinimoDomainEvent(ProductoId, cantidadDisponible, StockMinimo));
            }
            BajoMinimo = bajoMinimo;
        }
    }
}
