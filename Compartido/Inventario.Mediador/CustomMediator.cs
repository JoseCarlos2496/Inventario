using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Inventario.Mediador
{
    public class CustomMediator : ICustomMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomMediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();

            // Construimos el tipo de interfaz del manejador esperado: IRequestHandler<TRequest, TResponse>
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            var handler = _serviceProvider.GetService(handlerType)
                ?? throw new InvalidOperationException($"No se encontró un manejador registrado para {requestType.Name}");

            var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync))!;

            try
            {
                return await (Task<TResponse>)method.Invoke(handler, [request, cancellationToken])!;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                // Se relanza la excepción original (p. ej. de dominio) para que el manejador HTTP la traduzca.
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        public async Task PublishAsync(object evento, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(evento.GetType());
            var handlers = _serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(handlerType))
                as IEnumerable<object>;
            if (handlers is null) return;

            var method = handlerType.GetMethod("HandleAsync")!;
            var errores = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    await (Task)method.Invoke(handler, [evento, cancellationToken])!;
                }
                catch (TargetInvocationException ex) when (ex.InnerException is not null)
                {
                    errores.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    errores.Add(ex);
                }
            }

            if (errores.Count > 0)
                throw new AggregateException($"Fallaron {errores.Count} manejador(es) del evento {evento.GetType().Name}.", errores);
        }
    }
}
