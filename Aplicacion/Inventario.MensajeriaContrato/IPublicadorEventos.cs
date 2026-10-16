using Inventario.Contratos;

namespace Inventario.MensajeriaContrato
{
    /// <summary>
    /// Puerto hacia el transporte (RabbitMQ, log, ...). Lo usa el despachador de la Outbox.
    /// </summary>
    public interface IPublicadorEventos
    {
        Task PublicarAsync(SobreEvento sobre, CancellationToken cancellationToken);
    }
}
