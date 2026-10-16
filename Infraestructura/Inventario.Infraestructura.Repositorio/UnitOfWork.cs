using Inventario.Agregados;
using Inventario.Dominio.Comun;
using Inventario.Mediador;
using Inventario.RepositorioContrato;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infraestructura.Repositorio
{
    public class UnitOfWork : IUnitOfWork
    {
        // Un manejador puede provocar nuevos eventos de dominio; se corta para no entrar en un ciclo infinito.
        private const int MaxRondasEventos = 5;

        private readonly DbContext _dbContext;
        private readonly ICustomMediator _mediator;

        public UnitOfWork(DbContext dbContext, ICustomMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Los eventos de dominio se despachan ANTES de guardar: sus manejadores escriben en la Outbox
            // y todo (datos + inbox + outbox) se confirma en un solo SaveChanges.
            await DespacharEventosDominioAsync(cancellationToken);

            try
            {
                return await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ExcepcionConflicto(CatalogoErrores.ConflictoConcurrencia, "El stock cambió mientras se procesaba la operación. Intente nuevamente.");
            }
        }

        private async Task DespacharEventosDominioAsync(CancellationToken cancellationToken)
        {
            for (var ronda = 0; ronda < MaxRondasEventos; ronda++)
            {
                var agregados = _dbContext.ChangeTracker
                    .Entries<AgregadoRoot>()
                    .Where(x => x.Entity.DomainEvents.Count > 0)
                    .Select(x => x.Entity)
                    .ToList();

                if (agregados.Count == 0)
                {
                    return;
                }

                var eventos = agregados.SelectMany(x => x.DomainEvents).ToList();
                agregados.ForEach(agregado => agregado.ClearDomainEvents());

                foreach (var evento in eventos)
                {
                    await _mediator.PublishAsync(evento, cancellationToken);
                }
            }
        }
    }
}
