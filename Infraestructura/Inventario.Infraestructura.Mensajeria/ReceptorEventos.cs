using System.Text.Json;
using Inventario.Aplicacion.Command;
using Inventario.Aplicacion.Dto;
using Inventario.Contratos;
using Inventario.Contratos.Eventos.Entrada;
using Inventario.Dominio.Comun;
using Inventario.Mediador;

namespace Inventario.Infraestructura.Mensajeria
{
    /// <summary>
    /// Punto único de entrada de eventos externos (RabbitMQ o webhook HTTP): traduce el evento a un comando.
    /// La idempotencia la resuelve cada comando con el EventoId.
    /// </summary>
    public class ReceptorEventos
    {
        public static readonly IReadOnlyList<Type> TiposConsumidos =
        [
            typeof(OrdenCompraRecibidaEvent)
        ];

        private readonly ICustomMediator _mediator;

        public ReceptorEventos(ICustomMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<object> ProcesarAsync(SobreEvento sobre, CancellationToken cancellationToken)
        {
            IEventoIntegracion? evento;
            try
            {
                evento = CatalogoEventos.Desenvolver(sobre);
            }
            catch (JsonException ex)
            {
                throw new ExcepcionValidacion(CatalogoErrores.EventoInvalido, $"El contenido del evento {sobre.Tipo} v{sobre.Version} no es válido: {ex.Message}");
            }

            switch (evento)
            {
                case OrdenCompraRecibidaEvent e:
                    var lineas = (e.Lineas ?? []).Select(l => new LineaOrdenCompraDto(l.ProductoId, l.Sku, l.Cantidad)).ToList();
                    return await _mediator.SendAsync(new ProcesarOrdenCompraCommand(sobre.EventoId, e.OrdenCompraId, lineas), cancellationToken);

                default:
                    throw new ExcepcionValidacion(CatalogoErrores.EventoNoConsumido, $"Inventario no consume el evento {sobre.Tipo} v{sobre.Version}.");
            }
        }
    }
}
