namespace Inventario.RepositorioContrato
{
    /// <summary>
    /// Bandeja de entrada (inbox): recuerda qué eventos externos ya se aplicaron, para ser idempotentes.
    /// El registro se guarda en la misma transacción que el efecto del evento.
    /// </summary>
    public interface IRegistroEventosRecibidos
    {
        Task<bool> FueProcesadoAsync(Guid eventoId, CancellationToken cancellationToken);

        void Registrar(Guid eventoId, string tipo);
    }
}
