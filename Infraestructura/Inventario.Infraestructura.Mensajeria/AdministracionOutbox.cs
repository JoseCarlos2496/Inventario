using Inventario.Dominio.Comun;
using Inventario.Infraestructura.Modelo;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infraestructura.Mensajeria
{
    public sealed record EventoFallidoDto(Guid Id, string Tipo, int Intentos, string? UltimoError, DateTime OcurridoEn, DateTime? FallidoEn);

    /// <summary>
    /// Consulta y reencola eventos de la Outbox que agotaron sus intentos.
    /// </summary>
    public sealed class AdministracionOutbox
    {
        private readonly ModeloInventario _modelo;

        public AdministracionOutbox(ModeloInventario modelo)
        {
            _modelo = modelo;
        }

        public Task<List<EventoFallidoDto>> ListarFallidosAsync(CancellationToken cancellationToken)
        {
            return _modelo.Outbox.AsNoTracking()
                .Where(m => m.FallidoEn != null && m.ProcesadoEn == null)
                .OrderByDescending(m => m.FallidoEn)
                .Select(m => new EventoFallidoDto(m.Id, m.Tipo + ".v" + m.Version, m.Intentos, m.UltimoError, m.OcurridoEn, m.FallidoEn))
                .ToListAsync(cancellationToken);
        }

        public async Task ReintentarAsync(Guid id, CancellationToken cancellationToken)
        {
            var mensaje = await _modelo.Outbox.FirstOrDefaultAsync(m => m.Id == id, cancellationToken)
                ?? throw new ExcepcionNoEncontrado(CatalogoErrores.EventoOutboxNoEncontrado, $"No existe el evento {id} en la Outbox.");

            if (mensaje.FallidoEn is null)
            {
                throw new ExcepcionConflicto(CatalogoErrores.EventoNoFallido, $"El evento {id} no está fallido.");
            }

            mensaje.Reencolar();
            await _modelo.SaveChangesAsync(cancellationToken);
        }
    }
}
