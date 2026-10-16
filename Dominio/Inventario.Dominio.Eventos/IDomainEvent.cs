namespace Inventario.Dominio.Eventos
{
    public interface IDomainEvent
    {
        Guid Id { get; }

        DateTime FechaOcurrencia { get; }
    }

    public abstract record EventoDominio : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();

        public DateTime FechaOcurrencia { get; } = DateTime.UtcNow;
    }
}
