using Inventario.Dominio.Comun;
using Inventario.Dominio.Eventos;
using Inventario.ObjetosValor;

namespace Inventario.Agregados
{
    /// <summary>
    /// Unidades de un producto en una ubicación concreta (p. ej. 4 laptops en Bodega principal).
    /// El total del producto es la suma de sus existencias en todas las ubicaciones.
    /// </summary>
    public class Existencia : AgregadoRoot
    {
        public Guid ProductoId { get; private set; }

        public Guid UbicacionId { get; private set; }

        public int Cantidad { get; private set; }

        /// <summary>
        /// Token de concurrencia optimista: dos órdenes simultáneas no pueden descontar las mismas unidades.
        /// </summary>
        public int Version { get; private set; }

        private Existencia() { }

        private Existencia(Guid productoId, Guid ubicacionId)
        {
            if (productoId == Guid.Empty || ubicacionId == Guid.Empty)
            {
                throw new ExcepcionValidacion(CatalogoErrores.ExistenciaSinDatos);
            }
            ProductoId = productoId;
            UbicacionId = ubicacionId;
        }

        public static Existencia Crear(Guid productoId, Guid ubicacionId)
        {
            return new Existencia(productoId, ubicacionId);
        }

        public void Ingresar(Cantidad cantidad, Guid compraId)
        {
            Cantidad += cantidad.Valor;
            Actualizar();
            AddDomainEvent(new StockIngresadoDomainEvent(ProductoId, UbicacionId, cantidad.Valor, Cantidad, compraId));
        }

        public void Descontar(Cantidad cantidad)
        {
            if (cantidad.Valor > Cantidad)
            {
                throw new ExcepcionConflicto(CatalogoErrores.StockInsuficienteUbicacion,
                    $"La ubicación {UbicacionId} tiene {Cantidad} unidades del producto {ProductoId} (se intentó descontar {cantidad.Valor}).");
            }
            Cantidad -= cantidad.Valor;
            Actualizar();
        }

        private void Actualizar()
        {
            Version++;
            MarcarModificado();
        }
    }
}
