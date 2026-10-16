using RabbitMQ.Client;

namespace Inventario.Infraestructura.Mensajeria
{
    internal static class FabricaConexionRabbitMq
    {
        public static ConnectionFactory Crear(OpcionesRabbitMq opciones)
        {
            var fabrica = new ConnectionFactory
            {
                ClientProvidedName = "inventario",
                // La reconexión la controla el propio servicio (bucle de reintento) para no duplicar consumidores.
                AutomaticRecoveryEnabled = false,
                // Los brokers gratuitos en la nube cortan conexiones inactivas; el heartbeat las mantiene vivas.
                RequestedHeartbeat = TimeSpan.FromSeconds(30)
            };

            if (!string.IsNullOrWhiteSpace(opciones.Uri))
            {
                // amqps:// activa TLS automáticamente.
                fabrica.Uri = new Uri(opciones.Uri);
                return fabrica;
            }

            fabrica.HostName = opciones.HostName;
            fabrica.Port = opciones.Port;
            fabrica.UserName = opciones.UserName;
            fabrica.Password = opciones.Password;
            fabrica.VirtualHost = opciones.VirtualHost;
            return fabrica;
        }
    }
}
