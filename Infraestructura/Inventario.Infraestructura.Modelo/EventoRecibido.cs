namespace Inventario.Infraestructura.Modelo
{
    /// <summary>
    /// Inbox: evento externo ya aplicado. Su Id es el eventoId del sobre recibido.
    /// </summary>
    public class EventoRecibido
    {
        public Guid Id { get; private set; }

        public string Tipo { get; private set; } = null!;

        public DateTime RecibidoEn { get; private set; }

        private EventoRecibido() { }

        public static EventoRecibido Crear(Guid eventoId, string tipo)
        {
            return new EventoRecibido { Id = eventoId, Tipo = tipo, RecibidoEn = DateTime.UtcNow };
        }
    }
}
