namespace Inventario.Infraestructura.Mensajeria
{
    public sealed class OpcionesMensajeria
    {
        public const string Seccion = "Mensajeria";

        /// <summary>
        /// <c>Log</c>: sin broker, los eventos publicados solo se escriben en el log (desarrollo).
        /// <c>RabbitMq</c>: publica y consume por RabbitMQ.
        /// </summary>
        public string Proveedor { get; set; } = "Log";

        public int IntervaloOutboxSegundos { get; set; } = 2;

        public OpcionesRabbitMq RabbitMq { get; set; } = new();
    }

    public sealed class OpcionesRabbitMq
    {
        /// <summary>
        /// URL completa del broker (p. ej. la que entrega CloudAMQP: <c>amqps://usuario:clave@host/vhost</c>).
        /// Si se define, tiene prioridad sobre HostName/Port/UserName/Password/VirtualHost.
        /// No versionarla: usar la variable de entorno <c>Mensajeria__RabbitMq__Uri</c> o user-secrets.
        /// </summary>
        public string? Uri { get; set; }

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";

        /// <summary>Exchange topic compartido por todos los microservicios; la routing key es el nombre estable del evento.</summary>
        public string Exchange { get; set; } = "taller.eventos";

        /// <summary>Cola propia de Inventario con los eventos que consume.</summary>
        public string Cola { get; set; } = "inventario.eventos";

        public string ExchangeFallidos { get; set; } = "inventario.eventos.dlx";

        public string ColaFallidos { get; set; } = "inventario.eventos.fallidos";
    }
}
