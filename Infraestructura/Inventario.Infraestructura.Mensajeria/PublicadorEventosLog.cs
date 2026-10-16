using Inventario.Contratos;
using Inventario.MensajeriaContrato;
using Microsoft.Extensions.Logging;

namespace Inventario.Infraestructura.Mensajeria
{
    /// <summary>
    /// Publicador sin broker: útil mientras Recepción, Preparación y Despacho no existen. Solo deja constancia en el log.
    /// </summary>
    public sealed class PublicadorEventosLog : IPublicadorEventos
    {
        private readonly ILogger<PublicadorEventosLog> _logger;

        public PublicadorEventosLog(ILogger<PublicadorEventosLog> logger)
        {
            _logger = logger;
        }

        public Task PublicarAsync(SobreEvento sobre, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Evento publicado (sin broker) {Tipo}.v{Version} {EventoId}: {Payload}",
                sobre.Tipo, sobre.Version, sobre.EventoId, sobre.Payload.GetRawText());
            return Task.CompletedTask;
        }
    }
}
