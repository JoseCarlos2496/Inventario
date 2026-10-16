using Inventario.MensajeriaContrato;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Infraestructura.Mensajeria
{
    public static class RegistroMensajeria
    {
        public static IServiceCollection AddMensajeriaInventario(this IServiceCollection services, IConfiguration configuration)
        {
            var seccion = configuration.GetSection(OpcionesMensajeria.Seccion);
            services.Configure<OpcionesMensajeria>(seccion);
            var opciones = seccion.Get<OpcionesMensajeria>() ?? new OpcionesMensajeria();

            services.AddScoped<ReceptorEventos>();
            services.AddScoped<AdministracionOutbox>();
            services.AddHostedService<OutboxDispatcher>();

            if (string.Equals(opciones.Proveedor, "RabbitMq", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IPublicadorEventos, PublicadorEventosRabbitMq>();
                services.AddHostedService<ConsumidorEventosRabbitMq>();
            }
            else
            {
                services.AddSingleton<IPublicadorEventos, PublicadorEventosLog>();
            }

            return services;
        }
    }
}
