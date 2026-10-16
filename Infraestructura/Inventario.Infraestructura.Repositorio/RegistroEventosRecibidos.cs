using Inventario.Infraestructura.Modelo;
using Inventario.RepositorioContrato;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infraestructura.Repositorio
{
    public class RegistroEventosRecibidos : IRegistroEventosRecibidos
    {
        private readonly DbSet<EventoRecibido> _dbSet;

        public RegistroEventosRecibidos(DbContext dbContext)
        {
            _dbSet = dbContext.Set<EventoRecibido>();
        }

        public Task<bool> FueProcesadoAsync(Guid eventoId, CancellationToken cancellationToken)
        {
            return _dbSet.AnyAsync(e => e.Id == eventoId, cancellationToken);
        }

        public void Registrar(Guid eventoId, string tipo)
        {
            _dbSet.Add(EventoRecibido.Crear(eventoId, tipo));
        }
    }
}
