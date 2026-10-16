using Inventario.Contratos.Eventos.Salida;
using Inventario.Dominio.Eventos;
using Inventario.Mediador;
using Inventario.MensajeriaContrato;

namespace Inventario.Aplicacion.Notification.Handler
{
    /// <summary>
    /// Traduce eventos de dominio a eventos de integración y los deja en la Outbox, dentro de la misma transacción.
    /// </summary>
    public class PublicarEventosIntegracionHandler :
        IEventHandler<StockIngresadoDomainEvent>,
        IEventHandler<OrdenAsignadaDomainEvent>,
        IEventHandler<OrdenRechazadaDomainEvent>,
        IEventHandler<StockBajoMinimoDomainEvent>
    {
        private readonly IBandejaSalida _bandejaSalida;

        public PublicarEventosIntegracionHandler(IBandejaSalida bandejaSalida)
        {
            _bandejaSalida = bandejaSalida;
        }

        public Task HandleAsync(StockIngresadoDomainEvent evento, CancellationToken cancellationToken = default)
        {
            _bandejaSalida.Agregar(new StockIngresadoEvent(evento.ProductoId, evento.UbicacionId, evento.CantidadIngresada, evento.CantidadEnUbicacion, evento.CompraId));
            return Task.CompletedTask;
        }

        /// <summary>Hacia Preparación: una línea por producto con los puntos de recogida.</summary>
        public Task HandleAsync(OrdenAsignadaDomainEvent evento, CancellationToken cancellationToken = default)
        {
            var lineas = evento.Recogidas
                .GroupBy(r => r.ProductoId)
                .Select(g => new LineaPreparacion(
                    g.Key,
                    g.First().Sku,
                    g.First().NombreProducto,
                    g.Sum(r => r.Cantidad),
                    g.Select(r => new PuntoRecogida(r.UbicacionId, r.CodigoUbicacion, r.NombreUbicacion, r.TipoUbicacion, r.Cantidad)).ToList()))
                .ToList();

            _bandejaSalida.Agregar(new OrdenAsignadaEvent(evento.AsignacionId, evento.OrdenCompraId, lineas));
            return Task.CompletedTask;
        }

        /// <summary>Hacia Recepción: por qué no se puede surtir la orden.</summary>
        public Task HandleAsync(OrdenRechazadaDomainEvent evento, CancellationToken cancellationToken = default)
        {
            var errores = evento.Errores
                .Select(e => new ErrorOrden(e.Codigo, e.Mensaje, e.ProductoId, e.Sku, e.CantidadSolicitada, e.CantidadDisponible))
                .ToList();

            _bandejaSalida.Agregar(new OrdenRechazadaEvent(evento.AsignacionId, evento.OrdenCompraId, evento.Motivo, errores));
            return Task.CompletedTask;
        }

        public Task HandleAsync(StockBajoMinimoDomainEvent evento, CancellationToken cancellationToken = default)
        {
            _bandejaSalida.Agregar(new StockBajoMinimoEvent(evento.ProductoId, evento.CantidadDisponible, evento.StockMinimo));
            return Task.CompletedTask;
        }
    }
}
