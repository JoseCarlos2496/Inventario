using System.Reflection;
using Inventario.Mediador;

namespace Inventario.Api.Configuracion
{
    public static class RegistroMediador
    {
        /// <summary>
        /// Registra <see cref="ICustomMediator"/> y todos los IRequestHandler / IEventHandler de los ensamblados indicados.
        /// </summary>
        public static IServiceCollection AddCustomMediator(this IServiceCollection services, params Assembly[] ensamblados)
        {
            services.AddScoped<ICustomMediator, CustomMediator>();

            var interfacesManejador = new[] { typeof(IRequestHandler<,>), typeof(IEventHandler<>) };

            var registros = ensamblados
                .SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && interfacesManejador.Contains(i.GetGenericTypeDefinition()))
                    .Select(i => (Interfaz: i, Implementacion: t)));

            foreach (var (interfaz, implementacion) in registros)
            {
                services.AddScoped(interfaz, implementacion);
            }

            return services;
        }
    }
}
