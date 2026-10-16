namespace Inventario.Mediador
{
    /// <summary>Un evento puede tener cero o varios manejadores (a diferencia de un request, que tiene uno).</summary>
    public interface IEventHandler<in TEvento>
    {
        Task HandleAsync(TEvento evento, CancellationToken cancellationToken = default);
    }
}
