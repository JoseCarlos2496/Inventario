using Inventario.Dominio.Eventos;

namespace Inventario.Agregados
{
    public abstract class AgregadoRoot : Entidad
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        protected AgregadoRoot()
        {
        }

        protected AgregadoRoot(Guid id) : base(id)
        {
        }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
