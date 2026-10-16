using Inventario.Contratos;
using Inventario.Infraestructura.Modelo;
using Inventario.MensajeriaContrato;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infraestructura.Repositorio
{
    public class BandejaSalidaEf : IBandejaSalida
    {
        private readonly DbContext _dbContext;

        public BandejaSalidaEf(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Agregar(IEventoIntegracion evento)
        {
            var sobre = CatalogoEventos.Envolver(evento, MensajeOutbox.Origen);
            _dbContext.Set<MensajeOutbox>().Add(MensajeOutbox.Desde(sobre));
        }
    }
}
