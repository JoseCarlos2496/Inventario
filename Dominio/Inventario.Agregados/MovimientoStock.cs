using Inventario.ObjetosValor;

namespace Inventario.Agregados
{
    /// <summary>
    /// Registro inmutable (kardex) de cada cambio de stock, por ubicación.
    /// </summary>
    public class MovimientoStock : AgregadoRoot
    {
        public Guid ProductoId { get; private set; }

        public Guid UbicacionId { get; private set; }

        public TipoMovimiento Tipo { get; private set; }

        public int Cantidad { get; private set; }

        /// <summary>
        /// Documento que originó el movimiento: compra (entrada) u orden de compra surtida (salida).
        /// </summary>
        public Guid Referencia { get; private set; }

        public DateTime Fecha { get; private set; } = DateTime.UtcNow;

        private MovimientoStock() { }

        private MovimientoStock(Guid productoId, Guid ubicacionId, TipoMovimiento tipo, Cantidad cantidad, Guid referencia)
        {
            ProductoId = productoId;
            UbicacionId = ubicacionId;
            Tipo = tipo;
            Cantidad = cantidad.Valor;
            Referencia = referencia;
        }

        public static MovimientoStock Registrar(Guid productoId, Guid ubicacionId, TipoMovimiento tipo, Cantidad cantidad, Guid referencia)
        {
            return new MovimientoStock(productoId, ubicacionId, tipo, cantidad, referencia);
        }
    }
}
