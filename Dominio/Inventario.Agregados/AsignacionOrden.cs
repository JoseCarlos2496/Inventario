using Inventario.Dominio.Comun;
using Inventario.Dominio.Eventos;

namespace Inventario.Agregados
{
    /// <summary>
    /// Resultado de procesar una orden de compra que llegó de Recepción. Una por orden.
    /// <para><b>Asignada</b>: el stock ya se descontó y <see cref="Detalles"/> dice a Preparación qué recoger en cada ubicación.</para>
    /// <para><b>Rechazada</b>: no se descontó nada; <see cref="Errores"/> explica el porqué con códigos del catálogo.</para>
    /// </summary>
    public class AsignacionOrden : AgregadoRoot
    {
        private readonly List<AsignacionDetalle> _detalles = [];
        private readonly List<AsignacionError> _errores = [];

        public Guid OrdenCompraId { get; private set; }

        public EstadoAsignacion Estado { get; private set; }

        public string? Motivo { get; private set; }

        public IReadOnlyCollection<AsignacionDetalle> Detalles => _detalles.AsReadOnly();

        public IReadOnlyCollection<AsignacionError> Errores => _errores.AsReadOnly();

        private AsignacionOrden() { }

        private AsignacionOrden(Guid ordenCompraId, EstadoAsignacion estado, string? motivo)
        {
            if (ordenCompraId == Guid.Empty)
            {
                throw new ExcepcionValidacion(CatalogoErrores.OrdenSinIdentificador);
            }
            OrdenCompraId = ordenCompraId;
            Estado = estado;
            Motivo = motivo;
        }

        internal static AsignacionOrden Asignar(Guid ordenCompraId, IReadOnlyCollection<AsignacionDetalle> detalles)
        {
            var asignacion = new AsignacionOrden(ordenCompraId, EstadoAsignacion.Asignada, null);
            asignacion._detalles.AddRange(detalles);
            asignacion.AddDomainEvent(new OrdenAsignadaDomainEvent(
                asignacion.Id,
                ordenCompraId,
                detalles.Select(d => new RecogidaEvento(d.ProductoId, d.Sku, d.NombreProducto, d.UbicacionId, d.CodigoUbicacion, d.NombreUbicacion, d.TipoUbicacion.ToString(), d.Cantidad)).ToList()));
            return asignacion;
        }

        internal static AsignacionOrden Rechazar(Guid ordenCompraId, IReadOnlyCollection<AsignacionError> errores)
        {
            var motivo = errores.Count == 1
                ? errores.First().Mensaje
                : $"La orden tiene {errores.Count} problemas; ver errores.";
            var asignacion = new AsignacionOrden(ordenCompraId, EstadoAsignacion.Rechazada, motivo);
            asignacion._errores.AddRange(errores);
            asignacion.AddDomainEvent(new OrdenRechazadaDomainEvent(
                asignacion.Id,
                ordenCompraId,
                motivo,
                errores.Select(e => new ErrorOrdenEvento(e.Codigo, e.Mensaje, e.ProductoId, e.Sku, e.CantidadSolicitada, e.CantidadDisponible)).ToList()));
            return asignacion;
        }
    }

    /// <summary>
    /// Qué recoger y dónde: N unidades de un producto en una ubicación. Guarda copia de sku/nombres
    /// para que el pedido a Preparación no cambie si luego se renombra el producto o la ubicación.
    /// </summary>
    public class AsignacionDetalle : Entidad
    {
        public Guid AsignacionOrdenId { get; private set; }

        public Guid ProductoId { get; private set; }

        public string Sku { get; private set; } = null!;

        public string NombreProducto { get; private set; } = null!;

        public Guid UbicacionId { get; private set; }

        public string CodigoUbicacion { get; private set; } = null!;

        public string NombreUbicacion { get; private set; } = null!;

        public TipoUbicacion TipoUbicacion { get; private set; }

        public int Cantidad { get; private set; }

        private AsignacionDetalle() { }

        internal AsignacionDetalle(Producto producto, Ubicacion ubicacion, int cantidad)
        {
            ProductoId = producto.Id;
            Sku = producto.Sku.Valor;
            NombreProducto = producto.Nombre.Valor;
            UbicacionId = ubicacion.Id;
            CodigoUbicacion = ubicacion.Codigo;
            NombreUbicacion = ubicacion.Nombre.Valor;
            TipoUbicacion = ubicacion.Tipo;
            Cantidad = cantidad;
        }
    }

    /// <summary>
    /// Un motivo de rechazo (código ORD-1xx de <see cref="CatalogoErrores"/>) ligado, si aplica, a un producto.
    /// </summary>
    public class AsignacionError : Entidad
    {
        public const int LongitudMaximaMensaje = 500;

        public Guid AsignacionOrdenId { get; private set; }

        public string Codigo { get; private set; } = null!;

        public string Mensaje { get; private set; } = null!;

        public Guid? ProductoId { get; private set; }

        public string? Sku { get; private set; }

        public int CantidadSolicitada { get; private set; }

        public int CantidadDisponible { get; private set; }

        private AsignacionError() { }

        internal AsignacionError(DefinicionError error, string mensaje, Guid? productoId, string? sku, int cantidadSolicitada, int cantidadDisponible)
        {
            Codigo = error.Codigo;
            Mensaje = mensaje.Length > LongitudMaximaMensaje ? mensaje[..LongitudMaximaMensaje] : mensaje;
            ProductoId = productoId;
            Sku = sku;
            CantidadSolicitada = cantidadSolicitada;
            CantidadDisponible = cantidadDisponible;
        }
    }
}
