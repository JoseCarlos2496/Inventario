namespace Inventario.Mediador
{
    public interface ICustomMediator
    {
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Entrega un evento a todos sus manejadores. Si alguno falla, los demás igual se ejecutan y al final
        /// se lanza un <see cref="AggregateException"/>.
        /// </summary>
        Task PublishAsync(object evento, CancellationToken cancellationToken = default);
    }
}
