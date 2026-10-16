using Inventario.Contratos;

namespace Inventario.MensajeriaContrato
{
    /// <summary>
    /// Outbox: deja el evento de integración pendiente dentro de la unidad de trabajo actual.
    /// Se publica al broker en segundo plano solo si la transacción se confirma.
    /// </summary>
    public interface IBandejaSalida
    {
        void Agregar(IEventoIntegracion evento);
    }
}
